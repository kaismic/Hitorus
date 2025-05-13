using Hitorus.Data.DTOs;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hitorus.Data.Entities {
    [Index(nameof(Name))]
    public class TagFilter {
        public const int TAG_FILTER_NAME_MAX_LEN = 200;
        public int Id { get; set; }
        [MaxLength(TAG_FILTER_NAME_MAX_LEN), Required]
        public required string Name { get; set; }
        public ICollection<Tag> Tags { get; set; } = default!;
        public int SearchConfigurationId { get; set; }
        [Required] public SearchConfiguration SearchConfiguration { get; set; } = default!;

        public TagFilterDTO ToDTO() => new() {
            Id = Id,
            Name = Name,
            SearchConfigurationId = SearchConfiguration.Id
        };

        public TagFilterBuildDTO ToBuildDTO() => new() {
            Name = Name,
            Tags = Tags.Select(t => t.ToDTO())
        };
    }
}