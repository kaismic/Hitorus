namespace Hitorus.Data.DTOs;
public class BrowseQueryResult {
    public required int TotalGalleryCount { get; set; }
    public IEnumerable<int> GalleryIds { get; set; } = [];
}