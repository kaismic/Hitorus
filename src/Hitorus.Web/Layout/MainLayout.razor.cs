using Hitorus.Data.DTOs;
using Hitorus.Web.Components.Dialogs;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Hitorus.Web.Layout {
    public partial class MainLayout : LayoutComponentBase {
        [Inject] LanguageTypeService LTService { get; set; } = default!;
        [Inject] AppConfigurationService AppConfigService { get; set; } = default!;
        [Inject] SearchConfigurationService SearchConfigurationService { get; set; } = default!;
        [Inject] IDialogService DialogService { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;
        [Inject] IConfiguration HostConfiguration { get; set; } = default!;
        [Inject] IStringLocalizer<MainLayout> Localizer { get; set; } = default!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;

        private MudThemeProvider _mudThemeProvider = null!;

        private bool _isDarkMode;
        private bool _drawerOpen = true;
        private bool _isInitialized = false;
        private bool _hasRendered = false;
        private bool _connectionError = false;

        private void DrawerToggle() => _drawerOpen = !_drawerOpen;

        protected override async Task OnInitializedAsync() {
            try {
                await LTService.Load();
                await AppConfigService.Load();
                await SearchConfigurationService.Load();
                if (AppConfigService.Config.AppLaunchCount == 0) {
                    IEnumerable<TagFilterDTO> examples = await SearchConfigurationService.CreateExampleTagFilters(AppConfigService.DefaultBrowserLanguage);
                    foreach (TagFilterDTO dto in examples) {
                        SearchConfigurationService.Config.TagFilters.Add(dto);
                    }
                    await AppConfigService.UpdateLastUpdateCheckTime(DateTimeOffset.UtcNow);
                }
                await AppConfigService.IncrementAppLaunchCount();
                _isInitialized = true;
            } catch (HttpRequestException) {
                _connectionError = true;
            }
            StateHasChanged();
            _ = OnInitRenderComplete();
        }

        protected override void OnAfterRender(bool firstRender) {
            if (firstRender) {
                _hasRendered = true;
                _ = OnInitRenderComplete();
            }
        }

        private async Task OnInitRenderComplete() {
            if (_isInitialized && _hasRendered) {
                _isDarkMode = await _mudThemeProvider.GetSystemPreference();
                if (AppConfigService.Config.LastUpdateCheckTime.AddDays(HostConfiguration.GetValue<int>("UpdateCheckInterval")) < DateTimeOffset.UtcNow) {
                    Version? recentVersion = await AppConfigService.GetRecentVersion();
                    if (recentVersion != null && recentVersion > AppConfigurationService.CURRENT_APP_VERSION) {
                        Snackbar.Add(
                            string.Format(Localizer["NewVersionAvailable"], $"{recentVersion.Major}.{recentVersion.Minor}.{recentVersion.Build}"),
                            Severity.Success,
                            options => {
                                options.ShowCloseIcon = true;
                                options.CloseAfterNavigation = false;
                                options.ShowTransitionDuration = 0;
                                options.HideTransitionDuration = 500;
                                options.VisibleStateDuration = 10000;
                            }
                        );
                    }
                }
                int surveyPromptShowInterval = HostConfiguration.GetValue<int>("SurveyPromptShowInterval");
                if (AppConfigService.Config.ShowSurveyPrompt &&
                    AppConfigService.Config.AppLaunchCount >= surveyPromptShowInterval &&
                    AppConfigService.Config.AppLaunchCount % surveyPromptShowInterval == 0) {
                    DialogOptions dialogOptions = new() {
                        BackdropClick = false
                    };
                    IDialogReference dialogRef = await DialogService.ShowAsync<SurveyPromptDialog>(null, dialogOptions);
                    DialogResult result = (await dialogRef.Result)!;
                    bool doNotShowAgain = (bool)result.Data!;
                    if (doNotShowAgain) {
                        await AppConfigService.UpdateShowSurveyPrompt(false);
                    }
                }
            }
        }
    }
}
