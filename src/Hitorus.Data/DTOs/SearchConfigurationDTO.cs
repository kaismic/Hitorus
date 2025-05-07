namespace Hitorus.Data.DTOs
{
    public class SearchConfigurationDTO
    {
        public int Id { get; set; }
        public bool IsAutoSaveEnabled { get; set; }
        public required int SelectedTagFilterId { get; set; }
        public required IEnumerable<int> SelectedIncludeTagFilterIds { get; set; }
        public required IEnumerable<int> SelectedExcludeTagFilterIds { get; set; }
        public required string TitleSearchKeyword { get; set; }
        public required GalleryLanguageDTO SelectedLanguage { get; set; }
        public required GalleryTypeDTO SelectedType { get; set; }
        public required ICollection<TagFilterDTO> TagFilters { get; set; }
        public required ICollection<SearchFilterDTO> SearchFilters { get; set; }
    }
}