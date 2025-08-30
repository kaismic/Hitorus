using Hitorus.Data.DTOs;
using MudBlazor;

namespace Hitorus.Data.Entities;
public class BrowseConfiguration {
    public int Id { get; set; }
    public ICollection<Tag> Tags { get; set; } = [];
    public GalleryLanguage? SelectedLanguage { get; set; }
    public GalleryType? SelectedType { get; set; }
    public string TitleSearchKeyword { get; set; } = "";
    public int ItemsPerPage { get; set; }
    public bool AutoRefresh { get; set; }
    public GalleryProperty SelectedSortProperty { get; set; }
    public SortDirection SelectedSortDirection { get; set; }
    public int MinimumImageCount { get; set; }

    public BrowseConfigurationDTO ToDTO() => new() {
        Id = Id,
        Tags = [.. Tags.Select(t => t.ToDTO())],
        SelectedLanguage = SelectedLanguage?.ToDTO(),
        SelectedType = SelectedType?.ToDTO(),
        TitleSearchKeyword = TitleSearchKeyword,
        ItemsPerPage = ItemsPerPage,
        AutoRefresh = AutoRefresh,
        SelectedSortProperty = SelectedSortProperty,
        SelectedSortDirection = SelectedSortDirection,
        MinimumImageCount = MinimumImageCount,
    };
}