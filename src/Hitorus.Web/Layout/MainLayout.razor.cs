﻿using Blazored.LocalStorage;
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
                    await LTService.Load();
                    await SearchConfigurationService.Load();
                    if (AppConfigService.Config.AppLaunchCount == 0) {
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

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                bool? isDarkMode = LocalStorageService.GetItem<bool?>(LocalStorageKeys.IS_DARK_MODE);
                if (isDarkMode == null) {
                    _isDarkMode = await _mudThemeProvider.GetSystemDarkModeAsync();
                    await _mudThemeProvider.WatchSystemDarkModeAsync(OnSystemDarkModeChanged);
                } else {
                    _isDarkMode = isDarkMode.Value;
                }
                _hasRendered = true;
                _ = OnInitRenderComplete();
            }
        }

        private async Task OnInitRenderComplete() {
            if (_isInitialized && _hasRendered) {
                if (AppConfigService.Config.LastUpdateCheckTime.AddDays(HostConfiguration.GetValue<int>("UpdateCheckInterval")) < DateTimeOffset.UtcNow) {
                    Version? latestApiVersion = await AppConfigService.GetLatestApiVersion();
                    Version currentApiVersion = await AppConfigService.GetCurrentApiVersion();
                    if (latestApiVersion != null && latestApiVersion > currentApiVersion) {
                        Snackbar.Add(
                            string.Format(Localizer["NewApiVersionAvailable"], latestApiVersion.ToString(3)),
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

        private Task OnSystemDarkModeChanged(bool value) {
            _isDarkMode = value;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private void OnDarkModeButtonToggled(bool value) {
            _isDarkMode = value;
            LocalStorageService.SetItem(LocalStorageKeys.IS_DARK_MODE, value);
        }

        private void OnApiPortChangeRequested() {
            LocalStorageService.SetItem(LocalStorageKeys.API_PORT, _apiPort);
            NavigationManager.NavigateTo(NavigationManager.BaseUri, true);
        }
    }
}
