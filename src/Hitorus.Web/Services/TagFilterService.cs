using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class TagFilterService(HttpClient httpClient, SearchConfigurationService searchConfigurationService) {
        public async Task<TagFilterDTO> GetAsync(int tagFilterId) {
            return (await httpClient.GetFromJsonAsync<TagFilterDTO>($"?configId={searchConfigurationService.Config.Id}&tagFilterId={tagFilterId}"))!;
        }

        public async Task<int> CreateAsync(TagFilterBuildDTO dto) {
            var response = await httpClient.PostAsJsonAsync("", dto);
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<IEnumerable<TagFilterDTO>> GetAllAsync() {
            return (await httpClient.GetFromJsonAsync<IEnumerable<TagFilterDTO>>($"all?configId={searchConfigurationService.Config.Id}"))!;
        }

        public async Task<bool> DeleteAsync(IEnumerable<int> tagFilterIds) {
            try {
                var response = await httpClient.PostAsJsonAsync($"delete?configId={searchConfigurationService.Config.Id}", tagFilterIds);
                return response.IsSuccessStatusCode;
            } catch (HttpRequestException) {
                return false;
            }
        }

        public async Task<bool> UpdateNameAsync(int tagFilterId, string name) {
            try {
                var response = await httpClient.PatchAsync($"name?configId={searchConfigurationService.Config.Id}&tagFilterId={tagFilterId}",
                    JsonContent.Create(name));
                return response.IsSuccessStatusCode;
            } catch (HttpRequestException) {
                return false;
            }
        }

        public async Task<IEnumerable<TagDTO>> GetTagsAsync(int tagFilterId) {
            return (await httpClient.GetFromJsonAsync<IEnumerable<TagDTO>>($"tags?configId={searchConfigurationService.Config.Id}&tagFilterId={tagFilterId}"))!;
        }

        public async Task<bool> UpdateTagsAsync(int tagFilterId, IEnumerable<int> tagIds) {
            try {
                var response = await httpClient.PatchAsync($"tags?configId={searchConfigurationService.Config.Id}&tagFilterId={tagFilterId}", JsonContent.Create(tagIds));
                return response.IsSuccessStatusCode;
            } catch (HttpRequestException) {
                return false;
            }
        }

        public async Task<IEnumerable<TagDTO>> GetTagsUnionAsync(IEnumerable<int> tagFilterIds) {
            var response = await httpClient.PostAsJsonAsync($"tags-union?configId={searchConfigurationService.Config.Id}", tagFilterIds);
            return (await response.Content.ReadFromJsonAsync<IEnumerable<TagDTO>>())!;
        }

        public async Task<List<TagFilterDTO>> ImportTagFilters(IEnumerable<TagFilterBuildDTO> value) {
            var response = await httpClient.PostAsJsonAsync($"import?configId={searchConfigurationService.Config.Id}", value);
            return (await response.Content.ReadFromJsonAsync<List<TagFilterDTO>>())!;
        }
        
        public async Task<List<TagFilterBuildDTO>> ExportTagFilters(IEnumerable<int> ids) {
            var response = await httpClient.PostAsJsonAsync($"export?configId={searchConfigurationService.Config.Id}", ids);
            return (await response.Content.ReadFromJsonAsync<List<TagFilterBuildDTO>>())!;
        }
    }
}