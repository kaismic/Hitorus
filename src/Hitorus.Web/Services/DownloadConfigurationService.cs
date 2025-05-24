using Blazored.LocalStorage;
using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class DownloadConfigurationService {
        private bool _isLoaded = false;
        public DownloadConfigurationDTO Config { get; private set; } = new();


        private readonly HttpClient _httpClient;
        public DownloadConfigurationService(HttpClient httpClient, IConfiguration hostConfiguration, ISyncLocalStorageService localStorageService) {
            _httpClient = httpClient;
            _httpClient.BaseAddress = Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "DownloadConfigServicePath");
        }

        public async Task Load() {
            if (_isLoaded) {
                return;
            }
            Config = (await _httpClient.GetFromJsonAsync<DownloadConfigurationDTO>(""))!;
            _isLoaded = true;
        }

        public async Task<bool> UpdateParallelDownload(bool enable) {
            var response = await _httpClient.PatchAsync($"update-parallel-download?configId={Config.Id}", JsonContent.Create(enable));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateThreadNum(int threadNum) {
            var response = await _httpClient.PatchAsync($"update-thread-num?configId={Config.Id}", JsonContent.Create(threadNum));
            return response.IsSuccessStatusCode;
        }
    }
}
