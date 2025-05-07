using Hitorus.Data.DTOs;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hitorus.Data.Entities {
    [Index(nameof(Title))]
    [Index(nameof(Date))]
    [Index(nameof(LastDownloadTime))]
    public class Gallery {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? JapaneseTitle { get; set; }
        public DateTimeOffset Date { get; set; }
        public required int[] SceneIndexes { get; set; }
        [MaxLength(45)] public required int[] Related { get; set; } // 7 digits * 5 items + "[]" + ", " * (5 - 1)
        public DateTimeOffset LastDownloadTime { get; set; }
        [Required] public required GalleryLanguage Language { get; set; }
        [Required] public required GalleryType Type { get; set; }
        public required ICollection<GalleryImage> Images { get; set; }
        public required ICollection<Tag> Tags { get; set; }

        public BrowseGalleryDTO ToBrowseDTO() => new() {
            Id = Id,
            Title = Title,
            Date = Date,
            LastDownloadTime = LastDownloadTime,
            Language = Language.ToDTO(),
            Type = Type.ToDTO(),
            Tags = [.. Tags.Select(t => t.ToDTO())],
            Images = [.. Images.Select(g => g.ToDTO())],
        };

        public DownloadGalleryDTO ToDownloadDTO(int imagesCount) => new() {
            Title = Title,
            ImageCount = imagesCount
        };

        public ViewGalleryDTO ToViewDTO() => new() {
            Title = Title,
            Images = [.. Images.Select(g => g.ToDTO())],
        };
    }
}
