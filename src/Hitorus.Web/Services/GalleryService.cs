using Blazored.LocalStorage;
using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class GalleryService {
        private readonly HttpClient _httpClient;
        public GalleryService(HttpClient httpClient, IConfiguration hostConfiguration, ISyncLocalStorageService localStorageService) {
            _httpClient = httpClient;
            _httpClient.BaseAddress = Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "GalleryServicePath");
        }

        public async Task<DownloadGalleryDTO?> GetDownloadGalleryDTO(int id) {
            try {
                HttpResponseMessage response = await _httpClient.GetAsync($"download?id={id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<DownloadGalleryDTO>();
            } catch (HttpRequestException e) {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound) {
                    return null;
                }
                throw;
            }
        }

        public async Task<List<BrowseGalleryDTO>> GetBrowseGalleryDTOs(IEnumerable<int> ids) {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("browse", ids);
            return (await response.Content.ReadFromJsonAsync<List<BrowseGalleryDTO>>())!;
        }
        
        public async Task<ViewGalleryDTO?> GetViewGalleryDTO(int id) {
            try {
                HttpResponseMessage response = await _httpClient.GetAsync($"view?id={id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ViewGalleryDTO>();
            } catch (HttpRequestException e) {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound) {
                    return null;
                }
                throw;
            }
        }

        /// <summary>
        /// <paramref name="pageIndex"/> is 0-based
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        public async Task<BrowseQueryResult> GetBrowseQueryResult(int pageIndex, int configId) {
            return (await _httpClient.GetFromJsonAsync<BrowseQueryResult>($"browse-ids?pageIndex={pageIndex}&configId={configId}"))!;
        }

        public async Task<bool> DeleteGalleries(IEnumerable<int> ids) {
            var response = await _httpClient.PostAsync("delete-galleries", JsonContent.Create(ids));
            return response.IsSuccessStatusCode;
        }
    }
}
