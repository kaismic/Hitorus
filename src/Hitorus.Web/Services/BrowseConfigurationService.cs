using BitFaster.Caching.Lru;
using Blazored.LocalStorage;
using Hitorus.Data;
using Hitorus.Data.DTOs;
using MudBlazor;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class BrowseConfigurationService {
        private bool _isLoaded = false;
        public BrowseConfigurationDTO Config { get; private set; } = new();
        private const int GALLERY_CACHE_SIZE_FACTOR = 3; // GalleryCache capacity must be at least 3
        public FastConcurrentLru<int, BrowseGalleryDTO> GalleryCache { get; private set; } = default!;
        /// <summary>
        /// 1-based page number
        /// </summary>
        public int PageNum { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public List<BrowseGalleryDTO> Galleries { get; set; } = [];
        public bool[] Selections { get; set; } = [];

        private readonly HttpClient _httpClient;
        public BrowseConfigurationService(HttpClient httpClient, IConfiguration hostConfiguration, ISyncLocalStorageService localStorageService) {
            _httpClient = httpClient;
            _httpClient.BaseAddress = Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "BrowseServicePath");
        }

        public async Task Load() {
            if (_isLoaded) {
                return;
            }
            Config = (await _httpClient.GetFromJsonAsync<BrowseConfigurationDTO>(""))!;
            GalleryCache = new(GALLERY_CACHE_SIZE_FACTOR * Config.ItemsPerPage);
            _isLoaded = true;
        }

        public async Task<bool> AddTagsAsync(IEnumerable<int> tagIds) {
            var response = await _httpClient.PatchAsync($"add-tags?configId={Config.Id}", JsonContent.Create(tagIds));
            return response.IsSuccessStatusCode;
        }
        
        public async Task<bool> RemoveTagsAsync(IEnumerable<int> tagIds) {
            var response = await _httpClient.PatchAsync($"remove-tags?configId={Config.Id}", JsonContent.Create(tagIds));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTitleSearchKeywordAsync(string titleSearchKeyword) {
            var response = await _httpClient.PatchAsync($"title-search-keyword?configId={Config.Id}", JsonContent.Create(titleSearchKeyword));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateLanguageAsync(int languageId) {
            var response = await _httpClient.PatchAsync($"language?configId={Config.Id}", JsonContent.Create(languageId));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTypeAsync(int typeId) {
            var response = await _httpClient.PatchAsync($"type?configId={Config.Id}", JsonContent.Create(typeId));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateItemsPerPageAsync(int value) {
            var prev = GalleryCache.ToArray();
            GalleryCache = new(GALLERY_CACHE_SIZE_FACTOR * value);
            for (int i = 0; i < Math.Min(prev.Length, GalleryCache.Capacity); i++) {
                GalleryCache.AddOrUpdate(prev[i].Key, prev[i].Value);
            }
            var response = await _httpClient.PatchAsync($"items-per-page?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }
        
        public async Task<bool> UpdateAutoRefreshAsync(bool value) {
            var response = await _httpClient.PatchAsync($"auto-refresh?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSelectedSortPropertyAsync(GalleryProperty value) {
            var response = await _httpClient.PatchAsync($"sort-property?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateSelectedSortDirectionAsync(SortDirection value) {
            var response = await _httpClient.PatchAsync($"sort-direction?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }
    }
}
