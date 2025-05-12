using Hitorus.Data.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hitorus.Data.DTOs {
    public class OriginalGalleryInfoDTO {
        public static readonly JsonSerializerOptions SERIALIZER_OPTIONS = new(JsonSerializerDefaults.Web) {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            
        };
        public static readonly Dictionary<TagCategory, string> CATEGORY_PROP_KEY_DICT = new() {
            { TagCategory.Tag, "tag" },
            { TagCategory.Male, "male" },
            { TagCategory.Female, "female" },
            { TagCategory.Artist, "artist" },
            { TagCategory.Group, "group" },
            { TagCategory.Character, "character" },
            { TagCategory.Series, "parody" }
        };

        public int Id { get; set; }
        public required string Title { get; set; }
        public string? JapaneseTitle { get; set; }
        public required string Language { get; set; }
        public required string Type { get; set; }
        [JsonConverter(typeof(GalleryDateTimeOffsetConverter))]
        public DateTimeOffset Date { get; set; }
        public int[] SceneIndexes { get; set; } = [];
        public required ICollection<OriginalImageInfoDTO> Files { get; set; }
        public Dictionary<string, string>[]? Artists { get; set; }
        public Dictionary<string, string>[]? Groups { get; set; }
        public Dictionary<string, string>[]? Characters { get; set; }
        public Dictionary<string, string>[]? Parodys { get; set; }
        public required CompositeTag[] Tags { get; set; }

        public struct CompositeTag {
            public string Tag { get; set; }
            [JsonConverter(typeof(EmptyStringNumberJsonConverter))]
            public int Male { get; set; }
            [JsonConverter(typeof(EmptyStringNumberJsonConverter))]
            public int Female { get; set; }
        }

        private class EmptyStringNumberJsonConverter : JsonConverter<int> {
            public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                try {
                    string s = reader.GetString()!;
                    if (s.Length == 0) {
                        return 0;
                    }
                    return int.Parse(s);
                } catch (InvalidOperationException) {
                    return reader.GetInt32();
                }
            }
            public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options) => writer.WriteNumberValue(value);
        }

        private class GalleryDateTimeOffsetConverter : JsonConverter<DateTimeOffset> {
            public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                return DateTimeOffset.Parse(reader.GetString()!);
            }
            public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) {
                writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mmzzz"));
            }
        }
    }
}
