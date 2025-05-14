using BitFaster.Caching.Lru;
using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class BrowseConfigurationService(HttpClient httpClient) {
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
        public List<GallerySortDTO> ActiveSorts { get; set; } = [];

        public async Task Load() {
            if (_isLoaded) {
                return;
            }
            Config = (await httpClient.GetFromJsonAsync<BrowseConfigurationDTO>(""))!;
            GalleryCache = new(GALLERY_CACHE_SIZE_FACTOR * Config.ItemsPerPage);
            ActiveSorts = [.. Config.Sorts.Where(s => s.IsActive)];
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
            var prev = GalleryCache.ToArray();
            GalleryCache = new(GALLERY_CACHE_SIZE_FACTOR * value);
            for (int i = 0; i < Math.Min(prev.Length, GalleryCache.Capacity); i++) {
                GalleryCache.AddOrUpdate(prev[i].Key, prev[i].Value);
            }
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
