using Hitorus.Api.Download;
using Hitorus.Data;
using Microsoft.AspNetCore.Mvc;

namespace Hitorus.Api.Controllers {
    [ApiController]
    [Route("api/download-service")]
    public class DownloadServiceController(IEventBus<DownloadEventArgs> eventBus) : ControllerBase {
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
    }
}
