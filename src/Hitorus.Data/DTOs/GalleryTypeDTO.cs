using Hitorus.Data.Entities;

namespace Hitorus.Data.DTOs
{
    public class GalleryTypeDTO
    {
        public int Id { get; set; }
        public string Value { get; set; } = "";

        public GalleryType ToEntity() => new() {
            Id = Id,
            Value = Value
        };
    }
}
