using Hitorus.Data.DbContexts;
using Hitorus.Data.DTOs;
using Hitorus.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hitorus.Api.Controllers {
    [ApiController]
    [Route("api/search-filter")]
    public class SearchFilterController(HitomiContext context) : ControllerBase {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<int> CreateSearchFilter(int configId, [FromBody] SearchFilterDTO dto) {
            SearchConfiguration? config = context.SearchConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            GalleryLanguage? l = context.GalleryLanguages.Find(dto.Language.Id);
            if (l == null) {
                return NotFound("Language id is not valid.");
            }
            GalleryType? t = context.GalleryTypes.Find(dto.Type.Id);
            if (t == null) {
                return NotFound("Type id is not valid.");
            }
            SearchFilter sf = dto.ToEntity();
            sf.Language = l;
            sf.Type = t;
            config.SearchFilters.Add(sf);
            context.SaveChanges();
            return Ok(sf.Id);
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteSearchFilter(int configId, int searchFilterId) {
            SearchFilter? searchFilter = context.SearchFilters
                .Include(sf => sf.SearchConfiguration)
                .FirstOrDefault(sf => sf.Id == searchFilterId && sf.SearchConfiguration.Id == configId);
            if (searchFilter == null) {
                return NotFound();
            }
            context.SearchFilters.Remove(searchFilter);
            context.SaveChanges();
            return Ok();
        }

        [HttpDelete("clear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult ClearSearchFilter(int configId) {
            SearchConfiguration? config = context.SearchConfigurations
                .Include(c => c.SearchFilters)
                .FirstOrDefault(c => c.Id == configId);
            if (config == null) {
                return NotFound();
            }
            context.SearchFilters.RemoveRange(config.SearchFilters);
            context.SaveChanges();
            return Ok();
        }
    }
}
