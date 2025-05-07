using Hitorus.Data.Entities;

namespace Hitorus.Data.DTOs
{
    public class SearchFilterDTO
    {
        public int Id { get; set; }
        public required string TitleSearchKeyword { get; init; }
        public required string SearchLink { get; set; }
        public required GalleryLanguageDTO Language { get; init; }
        public required GalleryTypeDTO Type { get; init; }
        public required ICollection<LabeledTagCollectionDTO> LabeledTagCollections { get; set; }
        public int SearchConfigurationId { get; set; }

        public SearchFilter ToEntity() => new() {
            Id = Id,
            TitleSearchKeyword = TitleSearchKeyword,
            SearchLink = SearchLink,
            LabeledTagCollections = [.. LabeledTagCollections.Select(ltc => ltc.ToEntity())]
        };
    }
}
