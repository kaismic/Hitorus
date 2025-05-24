using Hitorus.Data.DbContexts;
using Hitorus.Data.DTOs;
using Hitorus.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Hitorus.Api.Controllers {
    [ApiController]
    [Route("api/app-config")]
    public class AppConfigurationController(HitomiContext context) : ControllerBase {
        public static readonly Version CURRENT_API_VERSION = Assembly.GetExecutingAssembly()!.GetName().Version!;

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<AppConfigurationDTO> GetConfiguration() {
            AppConfiguration config = context.AppConfigurations.First();
            return Ok(config.ToDTO());
        }

        [HttpGet("version")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<Version> GetApiVersion() => Ok(CURRENT_API_VERSION);

        [HttpPatch("app-theme-color")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateAppThemeColor(int configId, [FromBody] string color) {
            AppConfiguration? config = context.AppConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.AppThemeColor = color;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("app-launch-count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateAppLaunchCount(int configId, [FromBody] int value) {
            AppConfiguration? config = context.AppConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.AppLaunchCount = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("show-survey-prompt")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateShowSurveyPrompt(int configId, [FromBody] bool value) {
            AppConfiguration? config = context.AppConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.ShowSurveyPrompt = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("last-update-check-time")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateLastUpdateCheckTime(int configId, [FromBody] DateTimeOffset value) {
            AppConfiguration? config = context.AppConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.LastUpdateCheckTime = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("show-search-page-walkthrough")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateShowSearchPageWalkthrough(int configId, [FromBody] bool value) {
            AppConfiguration? config = context.AppConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.ShowSearchPageWalkthrough = value;
            context.SaveChanges();
            return Ok();
        }
    }
}
