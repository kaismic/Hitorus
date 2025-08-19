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

        public async Task<bool> UpdateMaxConcurrentDownloadCount(int value) {
            var response = await _httpClient.PatchAsync($"update-max-concurrent-download-count?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateDownloadThreadCount(int value) {
            var response = await _httpClient.PatchAsync($"update-download-thread-count?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdatePreferredFormat(string value) {
            var response = await _httpClient.PatchAsync($"update-preferred-format?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }
    }
}
