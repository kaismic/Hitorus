using Hitorus.Data.Entities;

namespace Hitorus.Data.DTOs {
    public class GalleryLanguageDTO {
        public int Id { get; set; }
        public bool IsAll { get; set; }
        public string EnglishName { get; set; } = "";
        public string LocalName { get; set; } = "";
        public GalleryLanguage ToEntity() => new() {
            Id = Id,
            IsAll = IsAll,
            EnglishName = EnglishName,
            LocalName = LocalName
        };
    }
}
