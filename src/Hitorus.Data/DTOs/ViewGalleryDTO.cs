namespace Hitorus.Data.DTOs;
public class ViewGalleryDTO {
    public required string Title { get; set; }
    public required ICollection<GalleryImageDTO> Images { get; set; }
}