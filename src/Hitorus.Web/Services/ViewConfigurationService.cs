using Blazored.LocalStorage;
using Hitorus.Data;
using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class ViewConfigurationService {
        public bool IsLoaded { get; private set; } = false;
        public ViewConfigurationDTO Config { get; private set; } = new();

        private readonly HttpClient _httpClient;
        public ViewConfigurationService(HttpClient httpClient, IConfiguration hostConfiguration, ISyncLocalStorageService localStorageService) {
            _httpClient = httpClient;
            _httpClient.BaseAddress = Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "ViewConfigServicePath");
        }

        public async Task Load() {
            if (IsLoaded) {
                return;
            }
            Config = (await _httpClient.GetFromJsonAsync<ViewConfigurationDTO>(""))!;
            IsLoaded = true;
        }

        public async Task<bool> UpdateViewModeAsync(ViewMode value) {
            var response = await _httpClient.PatchAsync($"view-mode?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdatePageTurnIntervalAsync(int value) {
            var response = await _httpClient.PatchAsync($"page-turn-interval?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAutoScrollModeAsync(AutoScrollMode value) {
            var response = await _httpClient.PatchAsJsonAsync($"auto-scroll-mode?configId={Config.Id}", value);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateScrollSpeedAsync(int value) {
            var response = await _httpClient.PatchAsJsonAsync($"scroll-speed?configId={Config.Id}", value);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateLoopAsync(bool value) {
            var response = await _httpClient.PatchAsJsonAsync($"loop?configId={Config.Id}", value);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateImageLayoutModeAsync(ImageLayoutMode value) {
            var response = await _httpClient.PatchAsJsonAsync($"image-layout-mode?configId={Config.Id}", value);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateViewDirectionAsync(ViewDirection value) {
            var response = await _httpClient.PatchAsJsonAsync($"view-direction?configId={Config.Id}", value);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateInvertClickNavigationAsync(bool value) {
            var response = await _httpClient.PatchAsJsonAsync($"invert-click-navigation?configId={Config.Id}", value);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateInvertKeyboardNavigationAsync(bool value) {
            var response = await _httpClient.PatchAsJsonAsync($"invert-keyboard-navigation?configId={Config.Id}", value);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> Import(ViewConfigurationDTO value) {
            var response = await _httpClient.PostAsync("import", JsonContent.Create(value));
            if (response.IsSuccessStatusCode) {
                int id = Config.Id;
                Config = value;
                Config.Id = id; // Preserve the original ID after import
            }
            return response.IsSuccessStatusCode;
        }
    }
}
