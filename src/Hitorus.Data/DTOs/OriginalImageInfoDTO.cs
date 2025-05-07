namespace Hitorus.Data.DTOs {
    public class OriginalImageInfoDTO {
        public required int Height { get; set; }
        public required int Width { get; set; }
        public required string Hash { get; set; }
        public int Hasavif { get; set; }
        public int Hasjxl { get; set; }
        public int Haswebp { get; set; }
    }
}
