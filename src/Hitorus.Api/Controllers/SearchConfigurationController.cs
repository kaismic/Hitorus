using Hitorus.Api.Localization;
using Hitorus.Api.Utilities;
using Hitorus.Data.DbContexts;
using Hitorus.Data.DTOs;
using Hitorus.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Hitorus.Api.Controllers {
    [ApiController]
    [Route("api/search-config")]
    public class SearchConfigurationController(HitomiContext context) : ControllerBase {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<SearchConfigurationDTO> GetConfiguration() {
            SearchConfiguration config =
                context.SearchConfigurations
                .Include(c => c.SelectedLanguage)
                .Include(c => c.SelectedType)
                .Include(c => c.TagFilters)
                .Include(c => c.SearchFilters)
                .ThenInclude(sf => sf.LabeledTagCollections)
                .First();
            return Ok(config.ToDTO());
        }

        [HttpPatch("enable-auto-save")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateAutoSave(int configId, bool enable) {
            SearchConfiguration? config = context.SearchConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            if (config.AutoSaveEnabled == enable) {
                return Ok();
            }
            config.AutoSaveEnabled = enable;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("tag-filter-collection")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateSelectedTagFilterCollection(int configId, bool isInclude, [FromBody] IEnumerable<int> tagFilterIds) {
            SearchConfiguration? config = context.SearchConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            if (isInclude) {
                config.SelectedIncludeTagFilterIds = tagFilterIds;
            } else {
                config.SelectedExcludeTagFilterIds = tagFilterIds;
            }
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("selected-tag-filter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateSelectedTagFilter(int configId, int tagFilterId) {
            SearchConfiguration? config = context.SearchConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            if (config.SelectedTagFilterId == tagFilterId) {
                return Ok();
            }
            config.SelectedTagFilterId = tagFilterId;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("title-search-keyword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateTitleSearchKeyword(int configId, [FromBody] string titleSearchKeyword) {
            SearchConfiguration? config = context.SearchConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            config.TitleSearchKeyword = titleSearchKeyword;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("language")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateLanguage(int configId, int languageId) {
            SearchConfiguration? config = context.SearchConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            GalleryLanguage? language = context.GalleryLanguages.Find(languageId);
            if (language == null) {
                return NotFound();
            }
            config.SelectedLanguage = language;
            context.SaveChanges();
            return Ok();
        }

        [HttpPatch("type")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult UpdateType(int configId, int typeId) {
            SearchConfiguration? config = context.SearchConfigurations.Find(configId);
            if (config == null) {
                return NotFound();
            }
            GalleryType? type = context.GalleryTypes.Find(typeId);
            if (type == null) {
                return NotFound();
            }
            config.SelectedType = type;
            context.SaveChanges();
            return Ok();
        }

        [HttpPost("create-examples")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<IEnumerable<TagFilterDTO>> CreateExampleTagFilters(string language) {
            SearchConfiguration searchConfig = context.SearchConfigurations.First();
            if (searchConfig.ExampleTagFiltersCreated) {
                return NoContent();
            }
            ExampleTagFilterNames.Culture = CultureInfo.GetCultureInfo(language);
            IEnumerable<TagFilter> examples = [
                new() {
                    Name = ExampleTagFilterNames.Name1,
                    Tags = [
                        TagUtility.GetTag(context.Tags, "full color", TagCategory.Tag)!,
                        TagUtility.GetTag(context.Tags, "very long hair", TagCategory.Female)!,
                    ]
                },
                new() {
                    Name = ExampleTagFilterNames.Name2,
                    Tags = [
                        TagUtility.GetTag(context.Tags, "glasses", TagCategory.Female)!,
                        TagUtility.GetTag(context.Tags, "sole male", TagCategory.Male)!,
                    ]
                },
                new() {
                    Name = ExampleTagFilterNames.Name3,
                    Tags = [
                        TagUtility.GetTag(context.Tags, "naruto", TagCategory.Series)!,
                        TagUtility.GetTag(context.Tags, "big breasts", TagCategory.Female)!,
                    ]
                },
                new() {
                    Name = ExampleTagFilterNames.Name4,
                    Tags = [
                        TagUtility.GetTag(context.Tags, "non-h imageset", TagCategory.Tag)!
                    ]
                }
            ];
            searchConfig.TagFilters.AddRange(examples);
            context.SaveChanges();
            searchConfig.ExampleTagFiltersCreated = true;
            context.SaveChanges();
            return Ok(examples.Select(tf => tf.ToDTO()));
        }
    }
}
