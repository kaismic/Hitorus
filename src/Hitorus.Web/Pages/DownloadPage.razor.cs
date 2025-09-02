using Hitorus.Web.Services;
using Hitorus.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Text.RegularExpressions;

namespace Hitorus.Web.Pages {
    public partial class DownloadPage {
        [Inject] ISnackbar Snackbar { get; set; } = default!;
        [Inject] DownloadConfigurationService DownloadConfigurationService { get; set; } = default!;
        [Inject] BrowseConfigurationService BrowseConfigurationService { get; set; } = default!;
        [Inject] DownloadClientManagerService DownloadManager { get; set; } = default!;
        [Inject] DownloadService DownloadService { get; set; } = default!;
        [Inject] IStringLocalizer<DownloadPage> Localizer { get; set; } = default!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;

        private string _inputText = "";
        private bool _isAutoImporting = false;

        private async Task OnMaxConcurrentDownloadCountChanged(int value) {
            DownloadConfigurationService.Config.MaxConcurrentDownloadCount = value;
            await DownloadConfigurationService.UpdateMaxConcurrentDownloadCount(value);
        }
        
        private async Task OnDownloadThreadCountChanged(int value) {
            DownloadConfigurationService.Config.DownloadThreadCount = value;
            await DownloadConfigurationService.UpdateDownloadThreadCount(value);
        }
        
        private async Task OnPreferredFormatChanged(string value) {
            DownloadConfigurationService.Config.PreferredFormat = value;
            await DownloadConfigurationService.UpdatePreferredFormat(value);
        }

        protected override async Task OnInitializedAsync() {
            await DownloadConfigurationService.Load();
            DownloadManager.DownloadPageStateHasChanged = StateHasChanged;
            if (!DownloadManager.IsHubConnectionOpen) {
                DownloadManager.OpenHubConnection();
            }
        }

        private async Task OnAutoImportButtonClick() {
            _isAutoImporting = true;
            int importCount = await DownloadService.AutoImportGalleries();
            Snackbar.Add(
                string.Format(Localizer["ImportSuccess"], importCount),
                Severity.Success,
                UiConstants.DEFAULT_SNACKBAR_OPTIONS
            );
            _isAutoImporting = false;
            BrowseConfigurationService.BrowsePageRefreshQueued = true;
        }

        [GeneratedRegex(@"\d{6,7}")]
        private static partial Regex IdPatternRegex();
        private void Download(DownloadAction action) {
            MatchCollection matches = IdPatternRegex().Matches(_inputText);
            if (matches.Count == 0) {
                Snackbar.Add(Localizer["InvalidInput"], Severity.Error, UiConstants.DEFAULT_SNACKBAR_OPTIONS);
                return;
            }
            _ = DownloadService.SendAction(action, matches.Select(m => int.Parse(m.Value)));
            _inputText = "";
        }
    }
}
