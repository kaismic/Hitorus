using Hitorus.Data.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Hitorus.Data.Entities
{
    public class SearchFilter
    {
        public int Id { get; set; }
        public required string TitleSearchKeyword { get; set; }
        public required string SearchLink { get; set; }
        public GalleryLanguage? Language { get; set; }
        public GalleryType? Type { get; set; }
        [Required] public SearchConfiguration SearchConfiguration { get; set; } = default!;
        public required ICollection<LabeledTagCollection> LabeledTagCollections { get; set; }

        public SearchFilterDTO ToDTO() => new() {
            Id = Id,
            Language = Language?.ToDTO(),
            Type = Type?.ToDTO(),
            TitleSearchKeyword = TitleSearchKeyword,
            LabeledTagCollections = [.. LabeledTagCollections.Select(ltc => ltc.ToDTO())],
            SearchLink = SearchLink,
            SearchConfigurationId = SearchConfiguration.Id
        };
    }
}
