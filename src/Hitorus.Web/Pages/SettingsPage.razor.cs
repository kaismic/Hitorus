using Hitorus.Data;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Hitorus.Web.Pages {
    public partial class SettingsPage {
        [Inject] private AppConfigurationService AppConfigurationService { get; set; } = default!;
        [Inject] private ViewConfigurationService ViewConfigurationService { get; set; } = default!;
        [Inject] NavigationManager NavigationManager { get; set; } = default!;
        [Inject] IStringLocalizer<SettingsPage> Localizer { get; set; } = default!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;

        private static readonly string[] SUPPORTED_LANGUAGES = ["en", "ko"];

        protected override async Task OnInitializedAsync() {
            await AppConfigurationService.Load();
            await ViewConfigurationService.Load();
        }

        private async Task OnAppLanguageChanged(string value) {
            try {
                AppConfigurationService.Config.AppLanguage = value;
                AppConfigurationService.ChangeAppLanguage(value);
                await AppConfigurationService.UpdateAppLanguage(value);
            } catch (CultureNotFoundException e) {
                Console.WriteLine(e.Message);
            }
            // Refresh the page if the new value does not match the initial app language or
            // is not english (default) since blazor uses satellite assembly and
            // the new language's satellite assembly must not have been loaded
            value = value.Length == 0 ? AppConfigurationService.DefaultBrowserLanguage : value;
            if (value != AppConfigurationService.InitialAppLanguage && !value.Contains("en")) {
                NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
            }
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
    }
}
