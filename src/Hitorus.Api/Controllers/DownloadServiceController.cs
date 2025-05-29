using Hitorus.Api.Download;
using Hitorus.Api.Utilities;
using Hitorus.Data;
using Hitorus.Data.DbContexts;
using Microsoft.AspNetCore.Mvc;

namespace Hitorus.Api.Controllers {
    [ApiController]
    [Route("api/download-service")]
    public class DownloadServiceController(IEventBus<DownloadEventArgs> eventBus, HitomiContext dbContext) : ControllerBase {
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult CreateDownloaders([FromBody] IEnumerable<int> galleryIds) {
            eventBus.Publish(new() {
                Action = DownloadAction.Create,
                GalleryIds = galleryIds,
            });
            return Ok();
        }

        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult StartDownloaders([FromBody] IEnumerable<int> galleryIds) {
            eventBus.Publish(new() {
                Action = DownloadAction.Start,
                GalleryIds = galleryIds,
            });
            return Ok();
        }

        [HttpPost("pause")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult PauseDownloaders([FromBody] IEnumerable<int> galleryIds) {
            eventBus.Publish(new() {
                Action = DownloadAction.Pause,
                GalleryIds = galleryIds,
            });
            return Ok();
        }

        [HttpPost("delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult DeleteDownloaders([FromBody] IEnumerable<int> galleryIds) {
            eventBus.Publish(new() {
                Action = DownloadAction.Delete,
                GalleryIds = galleryIds,
            });
            return Ok();
        }

        [HttpPost("import")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<int> ImportExistingGalleries() {
            IEnumerable<int> existingGalleryIds = GalleryIOUtility.GetExistingGalleries();
            if (!existingGalleryIds.Any()) {
                return Ok(0);
            }
            List<int> importingGalleries = [];
            foreach (int id in existingGalleryIds) {
                if (!dbContext.Galleries.Any(g => g.Id == id)) {
                    importingGalleries.Add(id);
                }
            }
            eventBus.Publish(new() {
                Action = DownloadAction.Start,
                GalleryIds = importingGalleries,
            });
            return Ok(importingGalleries.Count);
        }
    }
}
