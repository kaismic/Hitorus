using Hitorus.Data.Entities;

namespace Hitorus.Data.DTOs {
    public class ImageInfoSyncDTO {
        public required int Index { get; set; }
        public string? FileName { get; set; }
        public required bool IsPlayable { get; set; }
        public required string Hash { get; set; }
        public required int Height { get; set; }
        public required int Width { get; set; }
        public required string FileExtension { get; set; }
        //public GalleryImage ToImageInfo() => new() {
        //    Index = Index,
        //    FileName = FileName,
        //    Hash = Hash,
        //    Height = Height,
        //    Width = Width,
        //    FileExtension = FileExtension,
        //    IsPlayable = IsPlayable
        //};
    }
}
