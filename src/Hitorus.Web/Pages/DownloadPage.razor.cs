using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Text.RegularExpressions;

namespace Hitorus.Web.Pages {
    public partial class DownloadPage {
        [Inject] ISnackbar Snackbar { get; set; } = default!;
        [Inject] DownloadConfigurationService DownloadConfigurationService { get; set; } = default!;
        [Inject] DownloadClientManagerService DownloadManager { get; set; } = default!;
        [Inject] DownloadService DownloadService { get; set; } = default!;
        [Inject] IStringLocalizer<DownloadPage> Localizer { get; set; } = default!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;

        private string _inputText = "";
        private bool _isImporting = false;

        private async Task OnParallelDownloadChanged(bool value) {
            DownloadConfigurationService.Config.UseParallelDownload = value;
            await DownloadConfigurationService.UpdateParallelDownload(value);
        }
        
        private async Task OnThreadNumChanged(int value) {
            DownloadConfigurationService.Config.ThreadNum = value;
            await DownloadConfigurationService.UpdateThreadNum(value);
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

        private async Task OnImportButtonClick() {
            _isImporting = true;
            int importCount = await DownloadService.ImportGalleries();
            Snackbar.Add(
                string.Format(Localizer["ImportSuccess"], importCount),
                Severity.Success,
                UiConstants.DEFAULT_SNACKBAR_OPTIONS
            );
            _isImporting = false;

        }

        [GeneratedRegex(@"\d{6,7}")] private static partial Regex IdPatternRegex();
        private void OnDownloadButtonClick() {
            MatchCollection matches = IdPatternRegex().Matches(_inputText);
            if (matches.Count == 0) {
                Snackbar.Add(Localizer["InvalidInput"], Severity.Error, UiConstants.DEFAULT_SNACKBAR_OPTIONS);
                return;
            }
            _ = DownloadManager.AddDownloads(matches.Select(m => int.Parse(m.Value)));
            _inputText = "";
        }
    }
}
