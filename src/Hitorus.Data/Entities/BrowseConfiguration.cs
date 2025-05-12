using Hitorus.Data.DTOs;

namespace Hitorus.Data.Entities;
public class BrowseConfiguration {
    public int Id { get; set; }
    public ICollection<Tag> Tags { get; set; } = [];
    public int SelectedLanguageId { get; set; }
    public required GalleryLanguage SelectedLanguage { get; set; }
    public int SelectedTypeId { get; set; }
    public required GalleryType SelectedType { get; set; }
    public string TitleSearchKeyword { get; set; } = "";
    public required int ItemsPerPage { get; set; }
    public ICollection<GallerySort> Sorts { get; set; } = [];
    public bool AutoRefresh { get; set; }

    public BrowseConfigurationDTO ToDTO() => new() {
        Id = Id,
        Tags = [.. Tags.Select(t => t.ToDTO())],
        SelectedLanguage = SelectedLanguage.ToDTO(),
        SelectedType = SelectedType.ToDTO(),
        TitleSearchKeyword = TitleSearchKeyword,
        ItemsPerPage = ItemsPerPage,
        Sorts = [.. Sorts.Select(s => s.ToDTO())],
        AutoRefresh = AutoRefresh
    };
}