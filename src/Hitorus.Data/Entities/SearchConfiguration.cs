using Hitorus.Data.DTOs;

namespace Hitorus.Data.Entities;
public class SearchConfiguration {
    public int Id { get; set; }
    public bool IsAutoSaveEnabled { get; set; }
    public IEnumerable<int> SelectedIncludeTagFilterIds { get; set; } = [];
    public IEnumerable<int> SelectedExcludeTagFilterIds { get; set; } = [];
    public string TitleSearchKeyword { get; set; } = "";
    public int SelectedTagFilterId { get; set; }
    public required GalleryLanguage SelectedLanguage { get; set; }
    public required GalleryType SelectedType { get; set; }
    public List<TagFilter> TagFilters { get; set; } = [];
    public List<SearchFilter> SearchFilters { get; set; } = [];

    public SearchConfigurationDTO ToDTO() => new() {
        Id = Id,
        IsAutoSaveEnabled = IsAutoSaveEnabled,
        SelectedTagFilterId = SelectedTagFilterId,
        SelectedIncludeTagFilterIds = SelectedIncludeTagFilterIds,
        SelectedExcludeTagFilterIds = SelectedExcludeTagFilterIds,
        SelectedLanguage = SelectedLanguage.ToDTO(),
        SelectedType = SelectedType.ToDTO(),
        TitleSearchKeyword = TitleSearchKeyword,
        TagFilters = [.. TagFilters.Select(tf => tf.ToDTO())],
        SearchFilters = [.. SearchFilters.Select(sf => sf.ToDTO())]
    };
}