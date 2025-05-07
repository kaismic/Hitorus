using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class DownloadService(HttpClient httpClient) {
        public async Task<bool> CreateDownloaders(IEnumerable<int> galleryIds) {
            HttpResponseMessage response = await httpClient.PostAsync($"create", JsonContent.Create(galleryIds));
            return response.IsSuccessStatusCode;
        }
        
        public async Task<bool> StartDownloaders(IEnumerable<int> galleryIds) {
            HttpResponseMessage response = await httpClient.PostAsync($"start", JsonContent.Create(galleryIds));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PauseDownloaders(IEnumerable<int> galleryIds) {
            HttpResponseMessage response = await httpClient.PostAsync($"pause", JsonContent.Create(galleryIds));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDownloaders(IEnumerable<int> galleryIds) {
            HttpResponseMessage response = await httpClient.PostAsync($"delete", JsonContent.Create(galleryIds));
            return response.IsSuccessStatusCode;
        }
    }
}
