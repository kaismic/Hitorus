using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Text.RegularExpressions;

namespace Hitorus.Web.Pages {
    public partial class DownloadPage {
        [Inject] private ISnackbar Snackbar { get; set; } = default!;
        [Inject] private DownloadConfigurationService DownloadConfigurationService { get; set; } = default!;
        [Inject] private DownloadClientManagerService DownloadManager { get; set; } = default!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;

        private string _inputText = "";

        private async Task OnParallelDownloadChanged(bool value) {
            DownloadConfigurationService.Config.UseParallelDownload = value;
            await DownloadConfigurationService.UpdateParallelDownload(value);
        }
        
        private async Task OnThreadNumChanged(int value) {
            DownloadConfigurationService.Config.ThreadNum = value;
            await DownloadConfigurationService.UpdateThreadNum(value);
        }

        protected override async Task OnInitializedAsync() {
            await DownloadConfigurationService.Load();
            DownloadManager.DownloadPageStateHasChanged = StateHasChanged;
            if (!DownloadManager.IsHubConnectionOpen) {
                DownloadManager.OpenHubConnection();
            }
        }

        [GeneratedRegex(@"\d{6,7}")] private static partial Regex IdPatternRegex();
        private void OnDownloadButtonClick() {
            MatchCollection matches = IdPatternRegex().Matches(_inputText);
            if (matches.Count == 0) {
                Snackbar.Add("No valid IDs or URLs found in the input text.", Severity.Error, UiConstants.DEFAULT_SNACKBAR_OPTIONS);
                return;
            }
            _ = DownloadManager.AddDownloads(matches.Select(m => int.Parse(m.Value)));
            _inputText = "";
        }
    }
}
