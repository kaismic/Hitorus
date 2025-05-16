using Hitorus.Data.Entities;

namespace Hitorus.Data.DTOs
{
    public class SearchFilterDTO
    {
        public int Id { get; set; }
        public string TitleSearchKeyword { get; init; } = "";
        public required string SearchLink { get; set; }
        public GalleryLanguageDTO? Language { get; init; }
        public GalleryTypeDTO? Type { get; init; }
        public ICollection<LabeledTagCollectionDTO> LabeledTagCollections { get; set; } = [];
        public int SearchConfigurationId { get; set; }

        public SearchFilter ToEntity() => new() {
            Id = Id,
            TitleSearchKeyword = TitleSearchKeyword,
            SearchLink = SearchLink,
            LabeledTagCollections = [.. LabeledTagCollections.Select(ltc => ltc.ToEntity())]
        };
    }
}
