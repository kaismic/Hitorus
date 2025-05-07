using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class GalleryService(HttpClient httpClient) {
        public async Task<int> GetCount() {
            return await httpClient.GetFromJsonAsync<int>("count");
        }

        public async Task<DownloadGalleryDTO?> GetDownloadGalleryDTO(int id) {
            try {
                HttpResponseMessage response = await httpClient.GetAsync($"download?id={id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<DownloadGalleryDTO>();
            } catch (HttpRequestException e) {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound) {
                    return null;
                }
                throw;
            }
        }

        public async Task<ViewGalleryDTO?> GetViewGalleryDTO(int id) {
            try {
                HttpResponseMessage response = await httpClient.GetAsync($"view?id={id}");
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
            return (await httpClient.GetFromJsonAsync<BrowseQueryResult>($"browse-galleries?pageIndex={pageIndex}&configId={configId}"))!;
        }

        public async Task<bool> DeleteGalleries(IEnumerable<int> ids) {
            var response = await httpClient.PostAsync("delete-galleries", JsonContent.Create(ids));
            return response.IsSuccessStatusCode;
        }
    }
}
