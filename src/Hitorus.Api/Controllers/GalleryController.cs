using Hitorus.Api.Utilities;
using Hitorus.Data;
using Hitorus.Data.DbContexts;
using Hitorus.Data.DTOs;
using Hitorus.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.Xml.Linq;

namespace Hitorus.Api.Controllers {
    [ApiController]
    [Route("api/gallery")]
    public class GalleryController(HitomiContext context) : ControllerBase {
        [HttpGet("download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<DownloadGalleryDTO> GetDownloadGalleryDTO(int id) {
            Gallery? gallery = context.Galleries.Find(id);
            if (gallery == null) {
                return NotFound();
            }
            return Ok(gallery.ToDownloadDTO(context.Entry(gallery).Collection(g => g.Images).Query().Count()));
        }

        [HttpPost("browse")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<List<BrowseGalleryDTO>> GetBrowseGalleryDTOs([FromBody] ICollection<int> ids) {
            List<BrowseGalleryDTO> galleries = new(ids.Count);
            foreach (int id in ids) {
                Gallery? gallery = context.Galleries.Find(id);
                if (gallery != null) {
                    context.Entry(gallery).Collection(g => g.Tags).Load();
                    context.Entry(gallery).Collection(g => g.Images).Load();
                    galleries.Add(gallery.ToBrowseDTO());
                }
            }
            return Ok(galleries);
        }

        [HttpGet("view")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ViewGalleryDTO> GetViewGalleryDTO(int id) {
            Gallery? gallery = context.Galleries.Find(id);
            if (gallery == null) {
                return NotFound();
            }
            context.Entry(gallery).Collection(g => g.Images).Load();
            return Ok(gallery.ToViewDTO());
        }

        /// <summary>
        /// <paramref name="pageIndex"/> is 0-based 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="configId"></param>
        /// <returns></returns>
        [HttpGet("browse-ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<BrowseQueryResult> GetBrowseQueryResult(int pageIndex, int configId) {
            if (pageIndex < 0) {
                return BadRequest("Page index must be greater than or equal to 0.");
            }
            BrowseConfiguration? config = context.BrowseConfigurations.Find(configId);
            if (config == null) {
                return NotFound($"Browse configuration with ID {configId} not found.");
            }
            context.Entry(config).Collection(c => c.Tags).Load();
            context.Entry(config).Reference(c => c.SelectedLanguage).Load();
            context.Entry(config).Reference(c => c.SelectedType).Load();
            IEnumerable<Gallery> galleries =
                context.Galleries.AsNoTracking()
                .Include(g => g.Tags)
                .Include(g => g.Language)
                .Include(g => g.Type);
            if (config.SelectedLanguage != null) {
                galleries = galleries.Where(g => g.Language.Id == config.SelectedLanguage.Id);
            }
            if (config.SelectedType != null) {
                galleries = galleries.Where(g => g.Type.Id == config.SelectedType.Id);
            }
            if (!string.IsNullOrEmpty(config.TitleSearchKeyword)) {
                galleries = galleries.Where(g => g.Title.Contains(config.TitleSearchKeyword, StringComparison.InvariantCultureIgnoreCase));
            }
            IEnumerable<int> selectedTagIds = config.Tags.Select(t => t.Id);
            foreach (int tagId in selectedTagIds) {
                galleries = galleries.Where(g => g.Tags.Any(t => t.Id == tagId));
            }

            Func<Gallery, object> sortKeyFunc = config.SelectedSortProperty switch {
                GalleryProperty.Id => g => g.Id,
                GalleryProperty.Title => g => g.Title,
                GalleryProperty.UploadTime => g => g.Date,
                GalleryProperty.LastDownloadTime => g => g.LastDownloadTime,
                GalleryProperty.UserDefinedOrder => g => g.UserDefinedOrder,
                _ => throw new NotImplementedException(),
            };
            galleries = config.SelectedSortDirection == SortDirection.Ascending ?
                galleries.OrderBy(sortKeyFunc) :
                galleries.OrderByDescending(sortKeyFunc);

            int filteredCount = galleries.Count();
            BrowseQueryResult result = new() {
                TotalGalleryCount = filteredCount,
                GalleryIds = galleries
                    .Skip(pageIndex * config.ItemsPerPage)
                    .Take(config.ItemsPerPage)
                    .Select(g => g.Id)
            };
            return Ok(result);
        }

        [HttpPost("delete-galleries")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult DeleteGalleries([FromBody] IEnumerable<int> ids) {
            foreach (int id in ids) {
                Gallery? gallery = context.Galleries.Find(id);
                if (gallery != null) {
                    context.Galleries.Remove(gallery);
                }
                GalleryIOUtility.DeleteGalleryDirectory(id);
            }
            context.SaveChanges();
            return Ok();
        }

        [HttpGet("export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<ExportGalleryDTO>> ExportGalleries() {
            IEnumerable<Gallery> galleries =
                context.Galleries.AsNoTracking()
                .Include(g => g.Tags)
                .Include(g => g.Language)
                .Include(g => g.Type)
                .Include(g => g.Images);
            IEnumerable<ExportGalleryDTO> result = galleries.Select(g => g.ToExportDTO());
            return Ok(result);
        }


        [HttpPost("import")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> ImportGalleries([FromBody] IEnumerable<ExportGalleryDTO> galleries) {
            int count = 0;
            int maxOrder = context.Galleries
                .OrderByDescending(g => g.UserDefinedOrder)
                .Select(g => g.UserDefinedOrder)
                .FirstOrDefault();
            foreach (ExportGalleryDTO dto in galleries) {
                if (context.Galleries.Any(existing => existing.Id == dto.Id)) {
                    continue; // Skip if gallery already exists
                }
                Console.WriteLine("Importing gallery: " + dto.Id);
                GalleryLanguage? language = context.GalleryLanguages.FirstOrDefault(l => l.EnglishName == dto.Language);
                GalleryType? type = context.GalleryTypes.FirstOrDefault(t => t.Value == dto.Type);
                Console.WriteLine("Language: " + (language?.EnglishName ?? "Not found"));
                Console.WriteLine("Type: " + (type?.Value ?? "Not found"));
                if (language == null || type == null) {
                    continue; // Skip if language or type is not found
                }
                count++;
                Gallery gallery = new() {
                    Id = dto.Id,
                    Title = dto.Title,
                    JapaneseTitle = dto.JapaneseTitle,
                    Date = dto.Date,
                    LastDownloadTime = DateTimeOffset.UtcNow,
                    SceneIndexes = dto.SceneIndexes,
                    Language = language,
                    Type = type,
                    UserDefinedOrder = ++maxOrder,
                    Tags = [],
                    Images = [.. dto.Images.Select(i => new GalleryImage() {
                        Index = i.Index,
                        FileName = i.FileName,
                        Hasavif = i.Hasavif,
                        Haswebp = i.Haswebp,
                        Hasjxl = i.Hasjxl,
                        Hash = i.Hash,
                        Width = i.Width,
                        Height = i.Height,
                    })]
                };
                foreach (TagDTO dtoTag in dto.Tags) {
                    Tag? tag = context.Tags.FirstOrDefault(t => t.Category == dtoTag.Category && t.Value == dtoTag.Value);
                    if (tag == null) {
                        tag = new Tag { Category = dtoTag.Category, Value = dtoTag.Value, GalleryCount = dtoTag.GalleryCount };
                        context.Tags.Add(tag);
                    }
                    gallery.Tags.Add(tag);
                }
                context.Galleries.Add(gallery);
            }
            context.SaveChanges();
            return Ok(count);
        }
    }
}
