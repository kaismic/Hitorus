using Blazored.LocalStorage;
using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class SearchFilterService {
        private readonly HttpClient _httpClient;
        private readonly SearchConfigurationService _searchConfigService;
        public SearchFilterService(
            HttpClient httpClient,
            IConfiguration hostConfiguration,
            ISyncLocalStorageService localStorageService,
            SearchConfigurationService searchConfigService
        ) {
            _httpClient = httpClient;
            _httpClient.BaseAddress = Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "SearchConfigServicePath");
            _searchConfigService = searchConfigService;
        }

        public async Task<int> CreateAsync(SearchFilterDTO searchFilter) {
            var response = await _httpClient.PostAsJsonAsync($"?configId={_searchConfigService.Config.Id}", searchFilter);
            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<bool> DeleteAsync(int searchFilterId) {
            var response = await _httpClient.DeleteAsync($"?configId={_searchConfigService.Config.Id}&searchFilterId={searchFilterId}");
            return response.IsSuccessStatusCode;
        }
        
        public async Task<bool> ClearAsync() {
            var response = await _httpClient.DeleteAsync($"clear?configId={_searchConfigService.Config.Id}");
            return response.IsSuccessStatusCode;
        }
    }
}
