using Hitorus.Data;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor.Utilities;

namespace Hitorus.Web.Pages {
    public partial class SettingsPage {
        [CascadingParameter] private Action LayoutStateHasChanged { get; set; } = default!;
        [Inject] private AppConfigurationService AppConfigurationService { get; set; } = default!;
        [Inject] private ViewConfigurationService ViewConfigurationService { get; set; } = default!;
        [Inject] NavigationManager NavigationManager { get; set; } = default!;
        [Inject] IStringLocalizer<SettingsPage> Localizer { get; set; } = default!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;

        private static readonly string[] SUPPORTED_LANGUAGES = ["en", "ko"];

        private MudColor _appThemeColor = default!;

        protected override async Task OnInitializedAsync() {
            await AppConfigurationService.Load();
            await ViewConfigurationService.Load();
            _appThemeColor = new('#' + AppConfigurationService.Config.AppThemeColor);
        }

        private async Task OnAppLanguageChanged(string value) {
            await AppConfigurationService.UpdateAppLanguage(value);
            AppConfigurationService.ChangeAppLanguage(value);
            // Refresh the page if the new value does not match the initial app language or
            // is not english (default) since blazor uses satellite assembly and
            // the new language's satellite assembly must not have been loaded
            value = value.Length == 0 ? AppConfigurationService.DefaultBrowserLanguage : value;
            if (value != AppConfigurationService.InitialAppLanguage && !value.Contains("en")) {
                NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
            }
            LayoutStateHasChanged();
        }

        private async Task OnViewModeChanged(ViewMode value) {
            ViewConfigurationService.Config.ViewMode = value;
            await ViewConfigurationService.UpdateViewModeAsync(value);
        }

        private async Task OnPageTurnIntervalChanged(int value) {
            ViewConfigurationService.Config.PageTurnInterval = value;
            await ViewConfigurationService.UpdatePageTurnIntervalAsync(value);
        }

        private async Task OnAutoScrollModeChanged(AutoScrollMode value) {
            ViewConfigurationService.Config.AutoScrollMode = value;
            await ViewConfigurationService.UpdateAutoScrollModeAsync(value);
        }

        private async Task OnScrollSpeedChanged(int value) {
            ViewConfigurationService.Config.ScrollSpeed = value;
            await ViewConfigurationService.UpdateScrollSpeedAsync(value);
        }

        private async Task OnLoopChanged(bool value) {
            ViewConfigurationService.Config.Loop = value;
            await ViewConfigurationService.UpdateLoopAsync(value);
        }

        private async Task OnImageLayoutModeChanged(ImageLayoutMode value) {
            ViewConfigurationService.Config.ImageLayoutMode = value;
            await ViewConfigurationService.UpdateImageLayoutModeAsync(value);
        }

        private async Task OnViewDirectionChanged(ViewDirection value) {
            ViewConfigurationService.Config.ViewDirection = value;
            await ViewConfigurationService.UpdateViewDirectionAsync(value);
        }

        private async Task OnInvertClickNavigationChanged(bool value) {
            ViewConfigurationService.Config.InvertClickNavigation = value;
            await ViewConfigurationService.UpdateInvertClickNavigationAsync(value);
        }

        private async Task OnInvertKeyboardNavigationChanged(bool value) {
            ViewConfigurationService.Config.InvertKeyboardNavigation = value;
            await ViewConfigurationService.UpdateInvertKeyboardNavigationAsync(value);
        }

        /// <summary>
        /// The parameter <paramref name="value"/> is in the format of "#RRGGBBAA", e.g. "#22AA66FF".
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private async Task OnAppThemeColorChanged(string value) {
            // MudColor.Value uses rgba format whereas TonePalette uses argb format so we need to convert it appropriately.
            await AppConfigurationService.UpdateAppThemeColor(value[1..^2]);
            AppConfigurationService.SetAppThemeColors();
            LayoutStateHasChanged();
        }
    }
}
