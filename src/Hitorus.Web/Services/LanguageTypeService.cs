using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class LanguageTypeService(HttpClient httpClient) {
        public bool IsLoaded { get; private set; } = false;
        public List<GalleryTypeDTO> Types { get; private set; } = [];
        private Dictionary<int, string> _typeValueDict = [];
        public List<GalleryLanguageDTO> Languages { get; private set; } = [];
        private Dictionary<int, string> _languageLocalNameDict = [];

        public async Task Load() {
            Types = (await httpClient.GetFromJsonAsync<List<GalleryTypeDTO>>("types"))!;
            Languages = (await httpClient.GetFromJsonAsync<List<GalleryLanguageDTO>>("languages"))!;
            _typeValueDict = Types.ToDictionary(type => type.Id, type => type.Value);
            _languageLocalNameDict = Languages.ToDictionary(lang => lang.Id, lang => lang.LocalName);
            IsLoaded = true;
        }

        public string GetTypeValue(int id) {
            if (_typeValueDict.TryGetValue(id, out string? value)) {
                return value;
            } else {
                throw new KeyNotFoundException($"Type with id {id} not found");
            }
        }

        public string GetLanguageLocalName(int id) {
            if (_languageLocalNameDict.TryGetValue(id, out string? value)) {
                return value;
            } else {
                throw new KeyNotFoundException($"Language with id {id} not found");
            }
        }
    }
}
