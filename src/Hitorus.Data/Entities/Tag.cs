using Hitorus.Data.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Hitorus.Data.Entities {
    public enum TagCategory {
        Tag = 0,
        Artist = 1,
        Male = 2,
        Female = 3,
        Group = 4,
        Character = 5,
        Series = 6
    }

    [Index(nameof(Value))]
    [Index(nameof(Category), nameof(Value), nameof(GalleryCount))]
    [Index(nameof(Category), nameof(GalleryCount))]
    public partial class Tag {
        public static readonly TagCategory[] TAG_CATEGORIES = Enum.GetValues<TagCategory>();

        public int Id { get; set; }
        public required TagCategory Category { get; set; }
        public required string Value { get; set; }
        public int GalleryCount { get; set; }
        public ICollection<Gallery> Galleries { get; } = [];
        public ICollection<TagFilter> TagFilters { get; } = [];

        public TagDTO ToDTO() => new() {
            Id = Id,
            Category = Category,
            Value = Value,
            GalleryCount = GalleryCount,
        };
    }
}
