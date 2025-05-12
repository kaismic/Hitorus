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
            IEnumerable<int> selectedTagIds = config.Tags.Select(t => t.Id);
            context.Entry(config).Collection(c => c.Sorts).Load();
            IEnumerable<Gallery> galleries =
                context.Galleries.AsNoTracking()
                .Include(g => g.Tags);
            if (config.SelectedLanguageId != 1) { // not IsAll
                galleries = galleries.Where(g => g.Language.Id == config.SelectedLanguageId);
            }
            if (config.SelectedTypeId != 1) { // not IsAll
                galleries = galleries.Where(g => g.Type.Id == config.SelectedTypeId);
            }
            if (!string.IsNullOrEmpty(config.TitleSearchKeyword)) {
                galleries = galleries.Where(g => g.Title.Contains(config.TitleSearchKeyword, StringComparison.InvariantCultureIgnoreCase));
            }
            foreach (int tagId in selectedTagIds) {
                galleries = galleries.Where(g => g.Tags.Any(t => t.Id == tagId));
            }
            GallerySort[] activeSorts = [.. config.Sorts.Where(s => s.IsActive).OrderBy(s => s.RankIndex)];
            if (activeSorts.Length > 0) {
                IOrderedEnumerable<Gallery> orderedGalleries = SortGallery(galleries, activeSorts[0]);
                for (int i = 1; i < activeSorts.Length; i++) {
                    orderedGalleries = ThenSortGallery(orderedGalleries, activeSorts[i]);
                }
                galleries = orderedGalleries;
            }
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
                GalleryFileUtility.DeleteGalleryDir(id);
            }
            context.SaveChanges();
            return Ok();
        }
    }
}
