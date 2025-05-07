using Hitorus.Data.DbContexts;
using Hitorus.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hitorus.Api.Controllers {
    [ApiController]
    [Route("api/tag")]
    public class TagController(HitomiContext context) : ControllerBase {
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<Tag>> GetTags(TagCategory category, int count, string? start) {
            IQueryable<Tag> tags = context.Tags.AsNoTracking().Where(tag => tag.Category == category);
            if (start != null && start.Length > 0) {
                tags = tags.Where(tag => tag.Value.StartsWith(start));
            }
            return Ok(tags.OrderByDescending(tag => tag.GalleryCount).Take(count));
        }
    }
}
