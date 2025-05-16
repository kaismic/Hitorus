using Hitorus.Data.Entities;

namespace Hitorus.Data.DTOs
{
    public class LabeledTagCollectionDTO
    {
        public required TagCategory Category { get; set; }
        public required IEnumerable<string> IncludeTagValues { get; set; }
        public required IEnumerable<string> ExcludeTagValues { get; set; }

        public LabeledTagCollection ToEntity() => new() {
            Category = Category,
            IncludeTagValues = IncludeTagValues,
            ExcludeTagValues = ExcludeTagValues
        };
    }
}
