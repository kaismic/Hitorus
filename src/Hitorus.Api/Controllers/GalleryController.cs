using Hitorus.Api.Utilities;
using Hitorus.Data.DbContexts;
using Hitorus.Data.DTOs;
using Hitorus.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

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

        private static IOrderedEnumerable<Gallery> SortGallery(IEnumerable<Gallery> galleries, GallerySort sort) =>
            sort.SortDirection == SortDirection.Ascending ?
                galleries.OrderBy(GetSortKey(sort)) :
                galleries.OrderByDescending(GetSortKey(sort));

        private static IOrderedEnumerable<Gallery> ThenSortGallery(IOrderedEnumerable<Gallery> galleries, GallerySort sort) =>
            sort.SortDirection == SortDirection.Ascending ?
                galleries.ThenBy(GetSortKey(sort)) :
                galleries.ThenByDescending(GetSortKey(sort));

        private static Func<Gallery, object> GetSortKey(GallerySort sort) {
            return sort.Property switch {
                GalleryProperty.Id => g => g.Id,
                GalleryProperty.Title => g => g.Title,
                GalleryProperty.UploadTime => g => g.Date,
                GalleryProperty.LastDownloadTime => g => g.LastDownloadTime,
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// <paramref name="pageIndex"/> is 0-based 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="configId"></param>
        /// <returns></returns>
        [HttpGet("browse-galleries")]
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
            context.Entry(config).Reference(c => c.SelectedLanguage).Load();
            context.Entry(config).Reference(c => c.SelectedType).Load();
            context.Entry(config).Collection(c => c.Tags).Load();
            context.Entry(config).Collection(c => c.Sorts).Load();
            IEnumerable<Gallery> galleries =
                context.Galleries.AsNoTracking()
                .Include(g => g.Language)
                .Include(g => g.Type)
                .Include(g => g.Tags)
                .Include(g => g.Images);
            if (!config.SelectedLanguage.IsAll) {
                galleries = galleries.Where(g => g.Language.Id == config.SelectedLanguage.Id);
            }
            if (!config.SelectedType.IsAll) {
                galleries = galleries.Where(g => g.Type.Id == config.SelectedType.Id);
            }
            if (!string.IsNullOrEmpty(config.TitleSearchKeyword)) {
                galleries = galleries.Where(g => g.Title.Contains(config.TitleSearchKeyword, StringComparison.InvariantCultureIgnoreCase));
            }
            foreach (Tag tag in config.Tags) {
                galleries = galleries.Where(g => g.Tags.Any(t => t.Id == tag.Id));
            }
            GallerySort[] activeSorts = [.. config.Sorts.Where(s => s.IsActive).OrderBy(s => s.RankIndex)];
            if (activeSorts.Length > 0) {
                IOrderedEnumerable<Gallery> orderedGalleries = SortGallery(galleries, activeSorts[0]);
                for (int i = 1; i < activeSorts.Length; i++) {
                    orderedGalleries = ThenSortGallery(orderedGalleries, activeSorts[i]);
                }
                galleries = orderedGalleries;
            }
            BrowseQueryResult result = new() {
                TotalGalleryCount = galleries.Count(),
                Galleries = galleries
                    .Skip(pageIndex * config.ItemsPerPage)
                    .Take(config.ItemsPerPage)
                    .Select(g => g.ToBrowseDTO())
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
                GalleryFileUtility.DeleteGalleryDir(id);
            }
            context.SaveChanges();
            return Ok();
        }
    }
}
