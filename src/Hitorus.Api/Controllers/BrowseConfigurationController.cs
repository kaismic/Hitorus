using Hitorus.Data;
using Hitorus.Data.DbContexts;
using Hitorus.Data.DTOs;
using Hitorus.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using MudBlazor;

namespace Hitorus.Api.Controllers {
    [ApiController]
    [Route("api/browse-config")]
    public class BrowseConfigurationController(HitomiContext context) : ControllerBase {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<BrowseConfigurationDTO> GetConfiguration() {
            BrowseConfiguration config = context.BrowseConfigurations.First();
            context.Entry(config).Reference(c => c.SelectedLanguage).Load();
            context.Entry(config).Reference(c => c.SelectedType).Load();
            context.Entry(config).Collection(c => c.Tags).Load();
            return Ok(config.ToDTO());
        }

        [HttpPatch("add-tags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult AddTags(int configId, [FromBody] IEnumerable<int> tagIds) {
            BrowseConfiguration? config = context.BrowseConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            foreach (int tagId in tagIds) {
                Tag? tag = context.Tags.Find(tagId);
                if (tag != null) {
                    config.Tags.Add(tag);
                }
            }
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("remove-tags")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult RemoveTags(int configId, [FromBody] IEnumerable<int> tagIds) {
            BrowseConfiguration? config = context.BrowseConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            foreach (int tagId in tagIds) {
                Tag? tag = context.Tags.Find(tagId);
                if (tag != null) {
                    config.Tags.Remove(tag);
                }
            }
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("language")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateLanguage(int configId, [FromBody] int languageId) {
            BrowseConfiguration? config = context.BrowseConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            context.Entry(config).Reference(c => c.SelectedLanguage).Load();
            config.SelectedLanguage = context.GalleryLanguages.Find(languageId);
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("type")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateType(int configId, [FromBody] int typeId) {
            BrowseConfiguration? config = context.BrowseConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            context.Entry(config).Reference(c => c.SelectedType).Load();
            config.SelectedType = context.GalleryTypes.Find(typeId);
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("title-search-keyword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateTitleSearchKeyword(int configId, [FromBody] string titleSearchKeyword) {
            BrowseConfiguration? config = context.BrowseConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.TitleSearchKeyword = titleSearchKeyword;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("items-per-page")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateItemsPerPage(int configId, [FromBody] int value) {
            BrowseConfiguration? config = context.BrowseConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.ItemsPerPage = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("auto-refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateAutoRefresh(int configId, [FromBody] bool value) {
            BrowseConfiguration? config = context.BrowseConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.AutoRefresh = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("sort-property")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateSelectedSortProperty(int configId, [FromBody] GalleryProperty value) {
            BrowseConfiguration? config = context.BrowseConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.SelectedSortProperty = value;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("sort-direction")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateSelectedSortDirection(int configId, [FromBody] SortDirection value) {
            BrowseConfiguration? config = context.BrowseConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.SelectedSortDirection = value;
            context.SaveChanges();
            return Ok();
        }
    }
}
