using Hitorus.Data.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Hitorus.Data.Entities
{
    public class LabeledTagCollection
    {
        public long Id { get; set; }
        public required TagCategory Category { get; set; }
        public required IEnumerable<string> IncludeTagValues { get; set; }
        public required IEnumerable<string> ExcludeTagValues { get; set; }
        [Required] public SearchFilter SearchFilter { get; set; } = default!;

        public LabeledTagCollectionDTO ToDTO() => new() {
            Category = Category,
            IncludeTagValues = IncludeTagValues,
            ExcludeTagValues = ExcludeTagValues,
        };
    }
}
