using Blazored.LocalStorage;
using Hitorus.Data.DTOs;
using MaterialColorUtilities.Palettes;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;
using Octokit;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace Hitorus.Web.Services {
    public partial class AppConfigurationService {
        [GeneratedRegex("""v?(\d)+\.(\d)+\.(\d)+""")]
        private static partial Regex AppVersionRegex();

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _hostConfiguration;
        public AppConfigurationService(HttpClient httpClient, IConfiguration hostConfiguration, ISyncLocalStorageService localStorageService) {
            _httpClient = httpClient;
            _httpClient.BaseAddress = Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "AppConfigServicePath");
            _hostConfiguration = hostConfiguration;
        }

        public AppConfigurationDTO Config { get; private set; } = new();
        public MudColor LocalAppThemeColor { get; set; } = new();
        private bool _isLoaded = false;
        public Version CurrentApiVersion { get; private set; } = new(0, 0, 0);

        public MudTheme AppTheme { get; } = new();

        public async Task Load(bool force) {
            if (_isLoaded && !force) {
                return;
            }
            Config = (await _httpClient.GetFromJsonAsync<AppConfigurationDTO>(""))!;
            CurrentApiVersion = (await _httpClient.GetFromJsonAsync<Version>("version"))!;
            SetAppThemeColors();
            _isLoaded = true;
        }

        public async Task<Version?> GetLatestApiVersion() {
            string repoName = _hostConfiguration["RepositoryName"]!;
            string owner = _hostConfiguration["Developer"]!;
            GitHubClient client = new(new ProductHeaderValue(repoName));
            try {
                IReadOnlyList<Release> releases = await client.Repository.Release.GetAll(owner, repoName, new() { PageSize = 1, PageCount = 1 });
                if (releases.Count == 0) {
                    return null;
                }
                string latestTagName = releases[0].TagName;
                Match match = AppVersionRegex().Match(latestTagName);
                if (match.Success) {
                    int major = int.Parse(match.Groups[1].Value);
                    int minor = int.Parse(match.Groups[2].Value);
                    int build = int.Parse(match.Groups[3].Value);
                    return new(major, minor, build);
                }
            } catch (ApiException) {
                return null;
            }
            return null;
        }

        // References
        // https://m2.material.io/design/color/the-color-system.html#tools-for-picking-colors
        // https://m3.material.io/styles/color/system/how-the-system-works
        // Tone - 0 is black, 100 is white
        private const uint PALETTE_LIGHT_TONE = 60;
        private const uint PALETTE_DARK_TONE = 40;

        /// <summary>
        /// <see cref="ComponentBase.StateHasChanged"/> must be called after this method is called.
        /// </summary>
        public void SetAppThemeColors() {
            LocalAppThemeColor = new('#' + Config.AppThemeColor);
            // The parameter of CorePalette.Of uses ARGB format
            CorePalette palette = CorePalette.Of(Utilities.RgbToArgb(Config.AppThemeColor));
            List<MudColor> paletteValues = [.. new uint[] {
                palette.Primary[PALETTE_LIGHT_TONE],
                palette.Secondary[PALETTE_LIGHT_TONE],
                palette.Tertiary[PALETTE_LIGHT_TONE],
                palette.Primary[PALETTE_DARK_TONE],
                palette.Secondary[PALETTE_DARK_TONE],
                palette.Tertiary[PALETTE_DARK_TONE],
            }
            // the paletteValues are in ARGB format so we need to convert them to RGBA format
            .Select(value => new MudColor(Utilities.ArgbToRgba(value)))];
            AppTheme.PaletteDark = new() {
                Primary = paletteValues[0],
                Secondary = paletteValues[1],
                Tertiary = paletteValues[2],
                AppbarBackground = paletteValues[0],
            };
            AppTheme.PaletteLight = new() {
                Primary = paletteValues[3],
                Secondary = paletteValues[4],
                Tertiary = paletteValues[5],
                AppbarBackground = paletteValues[3],
            };
        }

        public async Task<bool> UpdateAppThemeColor(string color) {
            Config.AppThemeColor = color;
            var response = await _httpClient.PatchAsync($"app-theme-color?configId={Config.Id}", JsonContent.Create(color));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> IncrementAppLaunchCount() {
            Config.AppLaunchCount++;
            var response = await _httpClient.PatchAsync($"app-launch-count?configId={Config.Id}", JsonContent.Create(Config.AppLaunchCount));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateShowSurveyPrompt(bool value) {
            Config.ShowSurveyPrompt = value;
            var response = await _httpClient.PatchAsync($"show-survey-prompt?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateLastUpdateCheckTime(DateTimeOffset value) {
            Config.LastUpdateCheckTime = value;
            var response = await _httpClient.PatchAsync($"last-update-check-time?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }
        
        public async Task<bool> UpdateShowSearchPageWalkthrough(bool value) {
            Config.ShowSearchPageWalkthrough = value;
            var response = await _httpClient.PatchAsync($"show-search-page-walkthrough?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }
        
        public async Task<bool> Import(AppConfigurationDTO value) {
            var response = await _httpClient.PostAsync("import", JsonContent.Create(value));
            if (response.IsSuccessStatusCode) {
                await Load(true);
            }
            return response.IsSuccessStatusCode;
        }
    }
}
