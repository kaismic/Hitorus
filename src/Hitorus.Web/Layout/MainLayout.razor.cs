using Blazored.LocalStorage;
using Hitorus.Data.DTOs;
using Hitorus.Web.Components.Dialogs;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Globalization;

namespace Hitorus.Web.Layout {
    public partial class MainLayout : LayoutComponentBase {
        [Inject] LanguageTypeService LTService { get; set; } = default!;
        [Inject] AppConfigurationService AppConfigService { get; set; } = default!;
        [Inject] SearchConfigurationService SearchConfigurationService { get; set; } = default!;
        [Inject] NavigationManager NavigationManager { get; set; } = default!;
        [Inject] IDialogService DialogService { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;
        [Inject] IConfiguration HostConfiguration { get; set; } = default!;
        [Inject] ISyncLocalStorageService LocalStorageService { get; set; } = default!;
        [Inject] IStringLocalizer<MainLayout> Localizer { get; set; } = default!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;

        private MudThemeProvider _mudThemeProvider = null!;

        private bool _isDarkMode;
        private bool _drawerOpen = true;
        private bool _isInitialized = false;
        private bool _hasRendered = false;
        private bool _connectionError = false;
        private bool _incompatibleVersion = false;
        private Version _currentApiVersion = new();
        private int _initialApiPort;
        private int _apiPort;

        private void DrawerToggle() => _drawerOpen = !_drawerOpen;

        protected override async Task OnInitializedAsync() {
            _initialApiPort = Utilities.GetApiPort(HostConfiguration, LocalStorageService);
            _apiPort = _initialApiPort;
            try {
                await AppConfigService.Load();
                // need to drop the revision part of the version to compare with the minimum compatible version
                Version temp = await AppConfigService.GetCurrentApiVersion();
                _currentApiVersion = new(temp.Major, temp.Minor, temp.Build);
                Version minCompatibleVersion = new(HostConfiguration["MinCompatibleApiVersion"]!);
                if (_currentApiVersion < minCompatibleVersion) {
                    _incompatibleVersion = true;
                } else {
                    if (AppConfigService.Config.AppLaunchCount == 0) {
                        await LTService.Load();
                        await SearchConfigurationService.Load();
                        IEnumerable<TagFilterDTO> examples = await SearchConfigurationService.CreateExampleTagFilters(CultureInfo.CurrentCulture.Name);
                        foreach (TagFilterDTO dto in examples) {
                            SearchConfigurationService.Config.TagFilters.Add(dto);
                        }
                    }
                    await AppConfigService.UpdateLastUpdateCheckTime(DateTimeOffset.UtcNow);
                    await AppConfigService.IncrementAppLaunchCount();
                    _isInitialized = true;
                }
            } catch (HttpRequestException) {
                _connectionError = true;
            }
            StateHasChanged();
            if (!_incompatibleVersion && !_connectionError) {
                _ = OnInitRenderComplete();
            }
        }

        protected override void OnAfterRender(bool firstRender) {
            if (firstRender) {
                _hasRendered = true;
                _ = OnInitRenderComplete();
            }
        }

        private async Task OnInitRenderComplete() {
            if (_isInitialized && _hasRendered) {
                _isDarkMode = await _mudThemeProvider.GetSystemDarkModeAsync();
                await _mudThemeProvider.WatchSystemDarkModeAsync(OnSystemDarkModeChanged);
                if (AppConfigService.Config.LastUpdateCheckTime.AddDays(HostConfiguration.GetValue<int>("UpdateCheckInterval")) < DateTimeOffset.UtcNow) {
                    Version? latestApiVersion = await AppConfigService.GetLatestApiVersion();
                    Version currentApiVersion = await AppConfigService.GetCurrentApiVersion();
                    if (latestApiVersion != null && latestApiVersion > currentApiVersion) {
                        Snackbar.Add(
                            string.Format(Localizer["NewApiVersionAvailable"], $"{latestApiVersion.Major}.{latestApiVersion.Minor}.{latestApiVersion.Build}"),
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

        private Task OnSystemDarkModeChanged(bool isDarkMode) {
            _isDarkMode = isDarkMode;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private void OnApiPortChangeRequested() {
            LocalStorageService.SetItem(LocalStorageKeys.API_PORT, _apiPort);
            NavigationManager.NavigateTo(NavigationManager.BaseUri, true);
        }
    }
}
