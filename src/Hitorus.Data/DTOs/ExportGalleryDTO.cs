namespace Hitorus.Data.DTOs {
    public class ExportGalleryDTO {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? JapaneseTitle { get; set; }
        public DateTimeOffset Date { get; set; }
        public DateTimeOffset LastDownloadTime { get; set; }
        public required int[] SceneIndexes { get; set; }
        public required string Language { get; set; }
        public required string Type { get; set; }
        public required ICollection<TagDTO> Tags { get; set; }
        public required ICollection<GalleryImageDTO> Images { get; set; }
    }
}