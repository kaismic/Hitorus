using Hitorus.Data.DTOs;
using Octokit;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Hitorus.Web.Services {
    public partial class AppConfigurationService(HttpClient httpClient, IConfiguration hostConfiguration) {
        public static readonly Version CURRENT_APP_VERSION = Assembly.GetExecutingAssembly()!.GetName().Version!;
        [GeneratedRegex("""v?(\d)+\.(\d)+\.(\d)+""")]
        private static partial Regex AppVersionRegex();

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
        public AppConfigurationDTO Config { get; private set; } = new();

        public async Task Load() {
            if (_isLoaded) {
                return;
            }
            Config = (await httpClient.GetFromJsonAsync<AppConfigurationDTO>(""))!;
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
    }
}
