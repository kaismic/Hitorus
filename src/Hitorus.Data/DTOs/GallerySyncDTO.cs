namespace Hitorus.Data.DTOs {
    public class GallerySyncDTO {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public string? JapaneseTitle { get; set; }
        public DateTimeOffset Date { get; set; }
        public required int[] SceneIndexes { get; set; }
        public required int[] Related { get; set; }
        public required int GalleryLanguageId { get; set; }
        public required int GalleryTypeId { get; set; }
        public required IEnumerable<ImageInfoSyncDTO> Files { get; set; }
        public required IEnumerable<int> TagIds { get; set; }
    }
}
