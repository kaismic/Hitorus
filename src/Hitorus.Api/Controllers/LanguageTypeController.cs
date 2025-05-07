using Hitorus.Data.DbContexts;
using Hitorus.Data.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hitorus.Api.Controllers
{
    [ApiController]
    [Route("api/language-type")]
    public class LanguageTypeController(HitomiContext context) : ControllerBase {
        [HttpGet("languages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<GalleryLanguageDTO>> GetGalleryLanguages() {
            return Ok(context.GalleryLanguages.AsNoTracking().Select(l => l.ToDTO()));
        }

        [HttpGet("types")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<GalleryTypeDTO>> GetGalleryTypes() {
            return Ok(context.GalleryTypes.AsNoTracking().Select(t => t.ToDTO()));
        }
    }
}
