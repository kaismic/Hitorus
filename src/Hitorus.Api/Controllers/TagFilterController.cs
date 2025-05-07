using Hitorus.Data.DbContexts;
using Hitorus.Data.DTOs;
using Hitorus.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Hitorus.Api.Controllers {
    [ApiController]
    [Route("api/tag-filter")]
    public class TagFilterController(HitomiContext context) : ControllerBase {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<TagFilterDTO> GetTagFilter(int configId, int tagFilterId) {
            SearchConfiguration? config = context.SearchConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }

            context.Entry(config).Collection(c => c.TagFilters).Load();
            TagFilter? tagFilter = config.TagFilters.Find(tf => tf.Id == tagFilterId);
            if (tagFilter == null) {
                return NotFound();
            }
            return Ok(tagFilter.ToDTO());
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<int> CreateTagFilter([FromBody] TagFilterBuildDTO dto) {
            SearchConfiguration? config = context.SearchConfigurations.Find(dto.SearchConfigurationId);
            if (config == null) {
                return NotFound();
            }
            List<Tag> tags = [.. dto.Tags.Select(t => t.ToEntity())];
            TagFilter tagFilter = new() { Name = dto.Name, Tags = tags };
            context.Tags.AttachRange(tags);
            config.TagFilters.Add(tagFilter);
            context.SaveChanges();
            return Ok(tagFilter.Id);
        }

        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<TagFilterDTO>> GetAllTagFilters(int configId) {
            SearchConfiguration? config = context.SearchConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            context.Entry(config).Collection(c => c.TagFilters).Load();
            return Ok(config.TagFilters.Select(tf => tf.ToDTO()));
        }

        [HttpPost("delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<TagFilterDTO> DeleteTagFilters(int configId, [FromBody] IEnumerable<int> tagFilterIds) {
            context.TagFilters.RemoveRange(context.TagFilters.Where(tf => tagFilterIds.Contains(tf.Id)));
            // should use below when there are multiple configs (users)
            //context.TagFilters
            //    .Include(tf => tf.SearchConfiguration)
            //    .Where(tf => tagFilterIds.Contains(tf.Id) && tf.SearchConfiguration.Id == configId);
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("name")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateTagFilterName(int configId, int tagFilterId, [FromBody] string name) {
            SearchConfiguration? config = context.SearchConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            context.Entry(config).Collection(c => c.TagFilters).Load();
            TagFilter? tagFilter = config.TagFilters.Find(tf => tf.Id == tagFilterId);
            if (tagFilter == null) {
                return NotFound();
            }
            tagFilter.Name = name;
            context.SaveChanges();
            return Ok();
        }

        [HttpGet("tags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<TagDTO>> GetTagFilterTags(int configId, int tagFilterId) {
            SearchConfiguration? config = context.SearchConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            context.Entry(config).Collection(c => c.TagFilters).Load();
            TagFilter? tagFilter = config.TagFilters.Find(tf => tf.Id == tagFilterId);
            if (tagFilter == null) {
                return NotFound();
            }
            context.Entry(tagFilter).Collection(tf => tf.Tags).Load();
            return Ok(tagFilter.Tags.Select(tag => tag.ToDTO()));
        }

        [HttpPatch("tags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateTagFilterTags(int configId, int tagFilterId, [FromBody] IEnumerable<int> tagIds) {
            SearchConfiguration? config = context.SearchConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            context.Entry(config).Collection(c => c.TagFilters).Load();
            TagFilter? tagFilter = config.TagFilters.Find(tf => tf.Id == tagFilterId);
            if (tagFilter == null) {
                return NotFound();
            }
            context.Entry(tagFilter).Collection(tf => tf.Tags).Load();
            tagFilter.Tags.Clear();
            foreach (int id in tagIds) {
                Tag? tag = context.Tags.Find(id);
                if (tag != null) {
                    tagFilter.Tags.Add(tag);
                }
            }
            context.SaveChanges();
            return Ok();
        }

        // return union of tags for each tag filters
        [HttpPost("tags-union")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<IEnumerable<TagDTO>> GetTagFilterTagsUnion(int configId, [FromBody] IEnumerable<int> tagFilterIds) {
            SearchConfiguration? config = context.SearchConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            context.Entry(config).Collection(c => c.TagFilters).Load();
            IEnumerable<TagDTO>? tags = [];
            foreach (int tagFilterId in tagFilterIds) {
                TagFilter? tagFilter = config.TagFilters.Find(tf => tf.Id == tagFilterId);
                if (tagFilter != null) {
                    context.Entry(tagFilter).Collection(tf => tf.Tags).Load();
                    tags = tags.UnionBy(tagFilter.Tags.Select(t => t.ToDTO()), t => t.Id);
                }
            }
            if (!tags.Any()) {
                return NoContent();
            }
            return Ok(tags);
        }
    }
}
