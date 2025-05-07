using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class SearchFilterService(HttpClient httpClient, SearchConfigurationService searchConfigurationService) {
        public async Task<int> CreateAsync(SearchFilterDTO searchFilter) {
            var response = await httpClient.PostAsJsonAsync($"?configId={searchConfigurationService.Config.Id}", searchFilter);
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<bool> DeleteAsync(int searchFilterId) {
            var response = await httpClient.DeleteAsync($"?configId={searchConfigurationService.Config.Id}&searchFilterId={searchFilterId}");
            return response.IsSuccessStatusCode;
        }
        
        public async Task<bool> ClearAsync() {
            var response = await httpClient.DeleteAsync($"clear?configId={searchConfigurationService.Config.Id}");
            return response.IsSuccessStatusCode;
        }
    }
}
