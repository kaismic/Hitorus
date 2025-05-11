using Hitorus.Data.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Hitorus.Data.Entities {
    [Index(nameof(IsAll))]
    [Index(nameof(EnglishName))]
    public class GalleryLanguage {
        public int Id { get; set; }
        public required bool IsAll { get; set; }
        public required string EnglishName { get; set; }
        public required string LocalName { get; set; }

        public ICollection<Gallery> Galleries { get; } = [];
        public GalleryLanguageDTO ToDTO() => new() {
            Id = Id,
            IsAll = IsAll,
            EnglishName = EnglishName,
            LocalName = LocalName
        };
    }

}