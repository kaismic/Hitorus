using Blazored.LocalStorage;
using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class SearchConfigurationService {
        private bool _isLoaded = false;
        public bool IsInitTagFilterNull { get; private set; } = false;
        public SearchConfigurationDTO Config { get; private set; } = new();

        private readonly HttpClient _httpClient;
        public SearchConfigurationService(HttpClient httpClient, IConfiguration hostConfiguration, ISyncLocalStorageService localStorageService) {
            _httpClient = httpClient;
            _httpClient.BaseAddress = Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "SearchConfigServicePath");
        }

        public async Task Load() {
            if (_isLoaded) {
                return;
            }
            Config = (await _httpClient.GetFromJsonAsync<SearchConfigurationDTO>(""))!;
            if (Config.SelectedTagFilterId == default) {
                IsInitTagFilterNull = true;
            }
            _isLoaded = true;
        }

        public async Task<bool> UpdateAutoSaveEnabledAsync(bool enable) {
            var response = await _httpClient.PatchAsync($"enable-auto-save?configId={Config.Id}&enable={enable}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSelectedTagFilterCollectionAsync(bool isInclude, IEnumerable<int> tagFilterIds) {
            var response = await _httpClient.PatchAsync($"tag-filter-collection?configId={Config.Id}&isInclude={isInclude}", JsonContent.Create(tagFilterIds));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSelectedTagFilterAsync(int tagFilterId) {
            var response = await _httpClient.PatchAsync($"selected-tag-filter?configId={Config.Id}&tagFilterId={tagFilterId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTitleSearchKeywordAsync(string titleSearchKeyword) {
            var response = await _httpClient.PatchAsync($"title-search-keyword?configId={Config.Id}", JsonContent.Create(titleSearchKeyword));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateLanguageAsync(int languageId) {
            var response = await _httpClient.PatchAsync($"language?configId={Config.Id}&languageId={languageId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTypeAsync(int typeId) {
            var response = await _httpClient.PatchAsync($"type?configId={Config.Id}&typeId={typeId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<TagFilterDTO>> CreateExampleTagFilters(string language) {
            var response = await _httpClient.PostAsync($"create-examples?language={language}", null);
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent) {
                return [];
            }
            return (await response.Content.ReadFromJsonAsync<IEnumerable<TagFilterDTO>>())!;
        }
    }
}
