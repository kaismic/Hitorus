using Hitorus.Data.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Hitorus.Data.Entities {
    [Index(nameof(IsAll))]
    [Index(nameof(Value))]
    public class GalleryType {
        public int Id { get; set; }
        public required bool IsAll { get; set; }
        private string _value = null!;
        public required string Value {
            get {
                //if (IsAll) {
                //    throw new InvalidOperationException($"{nameof(Value)} must not be accessed when {nameof(IsAll)} is true");
                //}
                return _value;
            }
            set => _value = value;
        }
        public ICollection<Gallery> Galleries { get; } = [];

        public GalleryTypeDTO ToDTO() => new() {
            Id = Id,
            IsAll = IsAll,
            Value = Value
        };
    }
}