namespace Hitorus.Data.DTOs
{
    public class GalleryImageDTO
    {
        public long Id { get; set; }
        public int Index { get; set; }
        public required string FileName { get; set; }
        public required string Hash { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public required int Hasavif { get; set; }
        public required int Hasjxl { get; set; }
        public required int Haswebp { get; set; }
    }
}
