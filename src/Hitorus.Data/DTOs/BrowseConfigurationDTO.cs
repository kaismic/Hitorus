using MudBlazor;

namespace Hitorus.Data.DTOs
{
    public class BrowseConfigurationDTO
    {
        public int Id { get; set; }
        public List<TagDTO> Tags { get; set; } = [];
        public GalleryLanguageDTO? SelectedLanguage { get; set; }
        public GalleryTypeDTO? SelectedType { get; set; }
        public string TitleSearchKeyword { get; set; } = "";
        public int ItemsPerPage { get; set; }
        public bool AutoRefresh { get; set; }
        public GalleryProperty SelectedSortProperty { get; set; }
        public SortDirection SelectedSortDirection { get; set; }
        public int MinimumImageCount { get; set; }
    }
}