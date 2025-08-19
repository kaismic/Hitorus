using Blazored.LocalStorage;
using Hitorus.Data;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class DownloadService {
        private readonly HttpClient _httpClient;
        public DownloadService(HttpClient httpClient, IConfiguration hostConfiguration, ISyncLocalStorageService localStorageService) {
            _httpClient = httpClient;
            _httpClient.BaseAddress = Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "DownloadServicePath");
        }

        public async Task<bool> SendAction(DownloadAction action, IEnumerable<int> galleryIds) {
            HttpResponseMessage response = await _httpClient.PostAsync($"?action={action}", JsonContent.Create(galleryIds));
            return response.IsSuccessStatusCode;
        }
        
        public async Task<int> ImportGalleries() {
            HttpResponseMessage response = await _httpClient.PostAsync($"import", null);
            return await response.Content.ReadFromJsonAsync<int>();
        }
    }
}
