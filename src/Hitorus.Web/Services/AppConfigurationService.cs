using Hitorus.Data.DTOs;
using MaterialColorUtilities.Palettes;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;
using Octokit;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Hitorus.Web.Services {
    public partial class AppConfigurationService(HttpClient httpClient, IConfiguration hostConfiguration) {
        public static readonly Version CURRENT_APP_VERSION = Assembly.GetExecutingAssembly()!.GetName().Version!;
        [GeneratedRegex("""v?(\d)+\.(\d)+\.(\d)+""")]
        private static partial Regex AppVersionRegex();

        public AppConfigurationDTO Config { get; private set; } = new();
        private bool _isLoaded = false;
        /// <summary>
        /// This value is used to store initial browser language obtained from CultureInfo.CurrentCulture so that
        /// when user switches back to automatic language in the SettingsPage, we use this value to set CurrentCulture
        /// </summary>
        public string DefaultBrowserLanguage { get; set; } = "";
        /// <summary>
        /// This value is used to store initial app language from the API.
        /// </summary>
        public string InitialAppLanguage { get; set; } = "";

        public MudTheme AppTheme { get; } = new();

        public async Task Load() {
            if (_isLoaded) {
                return;
            }
            Config = (await httpClient.GetFromJsonAsync<AppConfigurationDTO>(""))!;
            SetAppThemeColors();
            _isLoaded = true;
        }

        public void ChangeAppLanguage(string value) {
            if (string.IsNullOrEmpty(value)) {
                Utilities.SetAppLanguage(DefaultBrowserLanguage);
            } else {
                Utilities.SetAppLanguage(value);
            }
        }

        public async Task<Version?> GetRecentVersion() {
            string repoName = hostConfiguration["RepositoryName"]!;
            string owner = hostConfiguration["Developer"]!;
            GitHubClient client = new(new ProductHeaderValue(repoName));
            try {
                IReadOnlyList<Release> releases = await client.Repository.Release.GetAll(owner, repoName, new() { PageSize = 1, PageCount = 1 });
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

        public async Task<bool> UpdateIsFirstLaunch(bool value) {
            var response = await httpClient.PatchAsync($"is-first-launch?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAppLanguage(string value) {
            var response = await httpClient.PatchAsync($"app-language?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAutoUpdateCheckTime(DateTimeOffset value) {
            var response = await httpClient.PatchAsync($"last-update-check-time?configId={Config.Id}", JsonContent.Create(value));
            return response.IsSuccessStatusCode;
        }
        
        public async Task<bool> UpdateAppThemeColor(string color) {
            var response = await httpClient.PatchAsync($"app-theme-color?configId={Config.Id}", JsonContent.Create(color));
            return response.IsSuccessStatusCode;
        }
    }
}
