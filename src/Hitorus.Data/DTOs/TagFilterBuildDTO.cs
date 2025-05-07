namespace Hitorus.Data.DTOs
{
    public class TagFilterBuildDTO
    {
        public required string Name { get; set; }
        public required IEnumerable<TagDTO> Tags { get; set; }
        public required int SearchConfigurationId { get; set; }

        public TagFilterDTO ToDTO() => new() {
            Name = Name,
            SearchConfigurationId = SearchConfigurationId
        };
    }
}
