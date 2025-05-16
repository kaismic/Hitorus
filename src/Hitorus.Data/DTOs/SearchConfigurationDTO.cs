namespace Hitorus.Data.DTOs;
public class SearchConfigurationDTO
{
    public int Id { get; set; }
    public bool AutoSaveEnabled { get; set; }
    public int SelectedTagFilterId { get; set; }
    public IEnumerable<int> SelectedIncludeTagFilterIds { get; set; } = [];
    public IEnumerable<int> SelectedExcludeTagFilterIds { get; set; } = [];
    public string TitleSearchKeyword { get; set; } = "";
    public GalleryLanguageDTO? SelectedLanguage { get; set; }
    public GalleryTypeDTO? SelectedType { get; set; }
    public ICollection<TagFilterDTO> TagFilters { get; set; } = [];
    public ICollection<SearchFilterDTO> SearchFilters { get; set; } = [];
}