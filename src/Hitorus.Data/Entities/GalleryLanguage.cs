using Hitorus.Data.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Hitorus.Data.Entities {
    [Index(nameof(IsAll))]
    [Index(nameof(EnglishName))]
    public class GalleryLanguage {
        public int Id { get; set; }
        public required bool IsAll { get; set; }
        public required string EnglishName { get; set; }
        private string _localName = null!;
        public required string LocalName {
            get {
                //if (IsAll) {
                //    throw new InvalidOperationException($"{nameof(LocalName)} must not be accessed when {nameof(IsAll)} is true");
                //}
                return _localName;
            }
            set => _localName = value;
        }

        public ICollection<Gallery> Galleries { get; } = [];
        public GalleryLanguageDTO ToDTO() => new() {
            Id = Id,
            IsAll = IsAll,
            EnglishName = EnglishName,
            LocalName = LocalName
        };
    }

}