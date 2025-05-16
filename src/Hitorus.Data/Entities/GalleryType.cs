using Hitorus.Data.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Hitorus.Data.Entities {
    [Index(nameof(Value))]
    public class GalleryType {
        public int Id { get; set; }
        public required string Value { get; set; }
        public ICollection<Gallery> Galleries { get; } = [];

        public GalleryTypeDTO ToDTO() => new() {
            Id = Id,
            Value = Value
        };
    }
}