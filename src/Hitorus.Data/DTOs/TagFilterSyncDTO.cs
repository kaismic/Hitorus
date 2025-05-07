using Hitorus.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Hitorus.Data.DTOs {
    public class TagFilterSyncDTO {
        public required string Name { get; set; }
        public required IEnumerable<int> TagIds { get; set; }

        public TagFilter ToTagFilter(DbSet<Tag> tags) {
            return new() {
                Name = Name,
                Tags = [.. TagIds.Select(id => tags.Find(id)!)]
            };
        }
    }
}