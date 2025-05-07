using Hitorus.Data.Entities;

namespace Hitorus.Data.DTOs
{
    public class LabeledTagCollectionDTO
    {
        public long Id { get; set; }
        public required TagCategory Category { get; set; }
        public required IEnumerable<string> IncludeTagValues { get; set; }
        public required IEnumerable<string> ExcludeTagValues { get; set; }
        public int SearchFilterId { get; set; }

        public LabeledTagCollection ToEntity() => new() {
            Id = Id,
            Category = Category,
            IncludeTagValues = IncludeTagValues,
            ExcludeTagValues = ExcludeTagValues
        };
    }
}
