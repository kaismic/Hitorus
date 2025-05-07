using Hitorus.Data.Entities;

namespace Hitorus.Data.DTOs {
    public class TagDTO {
        public int Id { get; set; }
        public required TagCategory Category { get; set; }
        public required string Value { get; set; }
        public int GalleryCount { get; set; }

        public Tag ToEntity() => new() {
            Id = Id,
            Category = Category,
            Value = Value,
            GalleryCount = GalleryCount
        };
    }
}
