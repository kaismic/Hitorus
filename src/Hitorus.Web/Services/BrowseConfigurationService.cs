using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class BrowseConfigurationService(HttpClient httpClient) {
        private bool _isLoaded = false;
        public BrowseConfigurationDTO Config { get; private set; } = new();

        public async Task Load() {
            if (_isLoaded) {
                return;
            }
            Config = (await httpClient.GetFromJsonAsync<BrowseConfigurationDTO>(""))!;
            _isLoaded = true;
        }


        public async Task<bool> AddTagsAsync(IEnumerable<int> tagIds) {
            var response = await httpClient.PatchAsync($"add-tags?configId={Config.Id}", JsonContent.Create(tagIds));
            return response.IsSuccessStatusCode;
        }
        
        public async Task<bool> RemoveTagsAsync(IEnumerable<int> tagIds) {
            var response = await httpClient.PatchAsync($"remove-tags?configId={Config.Id}", JsonContent.Create(tagIds));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTitleSearchKeywordAsync(string titleSearchKeyword) {
            var response = await httpClient.PatchAsync($"title-search-keyword?configId={Config.Id}", JsonContent.Create(titleSearchKeyword));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateLanguageAsync(int languageId) {
            var response = await httpClient.PatchAsync($"language?configId={Config.Id}", JsonContent.Create(languageId));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTypeAsync(int typeId) {
            var response = await httpClient.PatchAsync($"type?configId={Config.Id}", JsonContent.Create(typeId));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateItemsPerPageAsync(int value) {
            var response = await httpClient.PatchAsync($"items-per-page?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }
        
        public async Task<bool> UpdateAutoRefreshAsync(bool value) {
            var response = await httpClient.PatchAsync($"auto-refresh?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateGallerySorts(IEnumerable<GallerySortDTO> value) {
            var response = await httpClient.PatchAsync($"gallery-sorts?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }
    }
}
