using Blazored.LocalStorage;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class DownloadService {
        private readonly HttpClient _httpClient;
        public DownloadService(HttpClient httpClient, IConfiguration hostConfiguration, ISyncLocalStorageService localStorageService) {
            _httpClient = httpClient;
            _httpClient.BaseAddress = Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "DownloadServicePath");
        }

        public async Task<bool> CreateDownloaders(IEnumerable<int> galleryIds) {
            HttpResponseMessage response = await _httpClient.PostAsync($"create", JsonContent.Create(galleryIds));
            return response.IsSuccessStatusCode;
        }
        
        public async Task<bool> StartDownloaders(IEnumerable<int> galleryIds) {
            HttpResponseMessage response = await _httpClient.PostAsync($"start", JsonContent.Create(galleryIds));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PauseDownloaders(IEnumerable<int> galleryIds) {
            HttpResponseMessage response = await _httpClient.PostAsync($"pause", JsonContent.Create(galleryIds));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDownloaders(IEnumerable<int> galleryIds) {
            HttpResponseMessage response = await _httpClient.PostAsync($"delete", JsonContent.Create(galleryIds));
            return response.IsSuccessStatusCode;
        }

        public async Task<int> ImportGalleries() {
            HttpResponseMessage response = await _httpClient.PostAsync($"import", null);
            return await response.Content.ReadFromJsonAsync<int>();
        }
    }
}
