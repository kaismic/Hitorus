using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class DownloadConfigurationService(HttpClient httpClient) {
        private bool _isLoaded = false;
        public DownloadConfigurationDTO Config { get; private set; } = new();

        public async Task Load() {
            if (_isLoaded) {
                return;
            }
            Config = (await httpClient.GetFromJsonAsync<DownloadConfigurationDTO>(""))!;
            _isLoaded = true;
        }

        public async Task<bool> UpdateParallelDownload(bool enable) {
            var response = await httpClient.PatchAsync($"update-parallel-download?configId={Config.Id}", JsonContent.Create(enable));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateThreadNum(int threadNum) {
            var response = await httpClient.PatchAsync($"update-thread-num?configId={Config.Id}", JsonContent.Create(threadNum));
            return response.IsSuccessStatusCode;
        }
    }
}
