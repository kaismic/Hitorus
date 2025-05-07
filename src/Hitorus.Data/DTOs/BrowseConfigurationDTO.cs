namespace Hitorus.Data.DTOs
{
    public class BrowseConfigurationDTO
    {
        public int Id { get; set; }
        public List<TagDTO> Tags { get; set; } = [];
        public GalleryLanguageDTO SelectedLanguage { get; set; } = new();
        public GalleryTypeDTO SelectedType { get; set; } = new();
        public string TitleSearchKeyword { get; set; } = "";
        public int ItemsPerPage { get; set; }
        public ICollection<GallerySortDTO> Sorts { get; set; } = [];
        public bool AutoRefresh { get; set; }
    }
}
