namespace Hitorus.Data.DTOs;
public class BrowseQueryResult {
    public required int TotalGalleryCount { get; set; }
    public required IEnumerable<BrowseGalleryDTO> Galleries { get; set; }
}
