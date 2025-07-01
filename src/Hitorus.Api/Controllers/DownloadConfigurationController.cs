using Hitorus.Data.DbContexts;
using Hitorus.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Hitorus.Api.Controllers {
    [ApiController]
    [Route("api/download-config")]
    public class DownloadConfigurationController(HitomiContext context) : ControllerBase {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<DownloadConfiguration> GetConfiguration() {
            DownloadConfiguration config = context.DownloadConfigurations.First();
            return Ok(config.ToDTO());
        }

        [HttpPatch("max-concurrent-download-count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateMaxConcurrentDownloadCount(int configId, [FromBody] int value) {
            DownloadConfiguration? config = context.DownloadConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.MaxConcurrentDownloadCount = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("update-download-thread-count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateDownloadThreadCount(int configId, [FromBody] int value) {
            DownloadConfiguration? config = context.DownloadConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.DownloadThreadCount = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("update-preferred-format")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdatePreferredFormat(int configId, [FromBody] string value) {
            DownloadConfiguration? config = context.DownloadConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.PreferredFormat = value;
            context.SaveChanges();
            return Ok();
        }
    }
}
