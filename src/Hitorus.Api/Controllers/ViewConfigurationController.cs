using Hitorus.Data;
using Hitorus.Data.DbContexts;
using Hitorus.Data.DTOs;
using Hitorus.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Hitorus.Api.Controllers {
    [ApiController]
    [Route("api/view-config")]
    public class ViewConfigurationController(HitomiContext context) : ControllerBase {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ViewConfigurationDTO> GetConfiguration() {
            ViewConfiguration config = context.ViewConfigurations.First();
            return Ok(config.ToDTO());
        }

        [HttpPatch("view-mode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateViewMode(int configId, [FromBody] ViewMode value) {
            ViewConfiguration? config = context.ViewConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.ViewMode = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("page-turn-interval")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdatePageTurnInterval(int configId, [FromBody] int value) {
            ViewConfiguration? config = context.ViewConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.PageTurnInterval = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("auto-scroll-mode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateAutoScrollMode(int configId, [FromBody] AutoScrollMode value) {
            ViewConfiguration? config = context.ViewConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.AutoScrollMode = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("scroll-speed")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateScrollSpeed(int configId, [FromBody] int value) {
            ViewConfiguration? config = context.ViewConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.ScrollSpeed = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("loop")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateLoop(int configId, [FromBody] bool value) {
            ViewConfiguration? config = context.ViewConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.Loop = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("image-layout-mode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateImageLayoutMode(int configId, [FromBody] ImageLayoutMode value) {
            ViewConfiguration? config = context.ViewConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.ImageLayoutMode = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("view-direction")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateViewDirection(int configId, [FromBody] ViewDirection value) {
            ViewConfiguration? config = context.ViewConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.ViewDirection = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("invert-click-navigation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateInvertClickNavigation(int configId, [FromBody] bool value) {
            ViewConfiguration? config = context.ViewConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.InvertClickNavigation = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("invert-keyboard-navigation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateInvertKeyboardNavigation(int configId, [FromBody] bool value) {
            ViewConfiguration? config = context.ViewConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.InvertKeyboardNavigation = value;
            context.SaveChanges();
            return Ok();
        }


        [HttpPost("import")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult Import([FromBody] ViewConfigurationDTO value) {
            ViewConfiguration config = context.ViewConfigurations.First();
            config.ViewMode = value.ViewMode;
            config.Loop = value.Loop;
            config.ImageLayoutMode = value.ImageLayoutMode;
            config.ViewDirection = value.ViewDirection;
            config.PageTurnInterval = value.PageTurnInterval;
            config.AutoScrollMode = value.AutoScrollMode;
            config.ScrollSpeed = value.ScrollSpeed;
            config.InvertClickNavigation = value.InvertClickNavigation;
            config.InvertKeyboardNavigation = value.InvertKeyboardNavigation;
            context.SaveChanges();
            return Ok();
        }
    }
}
