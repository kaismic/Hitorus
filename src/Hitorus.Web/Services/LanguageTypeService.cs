using Blazored.LocalStorage;
using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class LanguageTypeService {
        private bool _isLoaded = false;
        public List<GalleryTypeDTO> Types { get; private set; } = [];
        private Dictionary<int, string> _typeValueDict = [];
        public List<GalleryLanguageDTO> Languages { get; private set; } = [];
        private Dictionary<int, string> _languageLocalNameDict = [];

        private readonly HttpClient _httpClient;
        public LanguageTypeService(HttpClient httpClient, IConfiguration hostConfiguration, ISyncLocalStorageService localStorageService) {
            _httpClient = httpClient;
            _httpClient.BaseAddress = Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "LanguageTypeServicePath");
        }

        public async Task Load() {
            if (_isLoaded) {
                return;
            }
            Types = (await _httpClient.GetFromJsonAsync<List<GalleryTypeDTO>>("types"))!;
            Languages = (await _httpClient.GetFromJsonAsync<List<GalleryLanguageDTO>>("languages"))!;
            _typeValueDict = Types.ToDictionary(type => type.Id, type => type.Value);
            _languageLocalNameDict = Languages.ToDictionary(lang => lang.Id, lang => lang.LocalName);
            _isLoaded = true;
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
