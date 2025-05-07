using Hitorus.Data.DTOs;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hitorus.Data.Entities {
    [Index(nameof(Index))]
    public class GalleryImage {
        public long Id { get; set; }
        public int Index { get; set; } // 1 based index
        public required string FileName { get; set; } // named by Index with the format string: "D" + Math.Floor(Math.Log10(Files.Count) + 1);
        public required string Hash { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public required int Hasavif { get; set; }
        public required int Hasjxl { get; set; }
        public required int Haswebp { get; set; }
        [Required] public Gallery Gallery { get; set; } = default!;

        public GalleryImageDTO ToDTO() => new() {
            Id = Id,
            Index = Index,
            FileName = FileName,
            Hash = Hash,
            Width = Width,
            Height = Height,
            Hasavif = Hasavif,
            Hasjxl = Hasjxl,
            Haswebp = Haswebp
        };
    }
}
