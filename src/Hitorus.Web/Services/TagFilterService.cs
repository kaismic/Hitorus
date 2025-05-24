using Blazored.LocalStorage;
using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class TagFilterService {
        private readonly HttpClient _httpClient;
        private readonly SearchConfigurationService _searchConfigService;
        public TagFilterService(
            HttpClient httpClient,
            IConfiguration hostConfiguration,
            ISyncLocalStorageService localStorageService,
            SearchConfigurationService searchConfigService
        ) {
            _httpClient = httpClient;
            _httpClient.BaseAddress = Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "TagFilterServicePath");
            _searchConfigService = searchConfigService;
        }

        public async Task<TagFilterDTO> GetAsync(int tagFilterId) {
            return (await _httpClient.GetFromJsonAsync<TagFilterDTO>($"?configId={_searchConfigService.Config.Id}&tagFilterId={tagFilterId}"))!;
        }

        public async Task<int> CreateAsync(TagFilterBuildDTO dto) {
            var response = await _httpClient.PostAsJsonAsync("", dto);
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<IEnumerable<TagFilterDTO>> GetAllAsync() {
            return (await _httpClient.GetFromJsonAsync<IEnumerable<TagFilterDTO>>($"all?configId={_searchConfigService.Config.Id}"))!;
        }

        public async Task<bool> DeleteAsync(IEnumerable<int> tagFilterIds) {
            try {
                var response = await _httpClient.PostAsJsonAsync($"delete?configId={_searchConfigService.Config.Id}", tagFilterIds);
                return response.IsSuccessStatusCode;
            } catch (HttpRequestException) {
                return false;
            }
        }

        public async Task<bool> UpdateNameAsync(int tagFilterId, string name) {
            try {
                var response = await _httpClient.PatchAsync($"name?configId={_searchConfigService.Config.Id}&tagFilterId={tagFilterId}",
                    JsonContent.Create(name));
                return response.IsSuccessStatusCode;
            } catch (HttpRequestException) {
                return false;
            }
        }

        public async Task<IEnumerable<TagDTO>> GetTagsAsync(int tagFilterId) {
            return (await _httpClient.GetFromJsonAsync<IEnumerable<TagDTO>>($"tags?configId={_searchConfigService.Config.Id}&tagFilterId={tagFilterId}"))!;
        }

        public async Task<bool> UpdateTagsAsync(int tagFilterId, IEnumerable<int> tagIds) {
            try {
                var response = await _httpClient.PatchAsync($"tags?configId={_searchConfigService.Config.Id}&tagFilterId={tagFilterId}", JsonContent.Create(tagIds));
                return response.IsSuccessStatusCode;
            } catch (HttpRequestException) {
                return false;
            }
        }

        public async Task<IEnumerable<TagDTO>> GetTagsUnionAsync(IEnumerable<int> tagFilterIds) {
            var response = await _httpClient.PostAsJsonAsync($"tags-union?configId={_searchConfigService.Config.Id}", tagFilterIds);
            return (await response.Content.ReadFromJsonAsync<IEnumerable<TagDTO>>())!;
        }

        public async Task<List<TagFilterDTO>> ImportTagFilters(IEnumerable<TagFilterBuildDTO> value) {
            var response = await _httpClient.PostAsJsonAsync($"import?configId={_searchConfigService.Config.Id}", value);
            return (await response.Content.ReadFromJsonAsync<List<TagFilterDTO>>())!;
        }
        
        public async Task<List<TagFilterBuildDTO>> ExportTagFilters(IEnumerable<int> ids) {
            var response = await _httpClient.PostAsJsonAsync($"export?configId={_searchConfigService.Config.Id}", ids);
            return (await response.Content.ReadFromJsonAsync<List<TagFilterBuildDTO>>())!;
        }
    }
}