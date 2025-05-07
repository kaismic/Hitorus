namespace Hitorus.Data.DTOs
{
    public class BrowseGalleryDTO {
        public int Id { get; set; }
        public required string Title { get; set; }
        public DateTimeOffset Date { get; set; }
        public DateTimeOffset LastDownloadTime { get; set; }
        public required GalleryLanguageDTO Language { get; set; }
        public required GalleryTypeDTO Type { get; set; }
        public required ICollection<TagDTO> Tags { get; set; }
        public required ICollection<GalleryImageDTO> Images { get; set; }
    }
}
