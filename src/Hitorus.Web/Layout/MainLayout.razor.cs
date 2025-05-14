using Hitorus.Data;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Hitorus.Web.Layout {
    public partial class MainLayout : LayoutComponentBase, IAsyncDisposable {
        [Inject] public NavigationManager NavigationManager { get; set; } = default!;
        [Inject] public LanguageTypeService LanguageTypeService { get; set; } = default!;
        [Inject] private IConfiguration HostConfiguration { get; set; } = default!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;

        // Helper websites
        // https://coolors.co/palettes/popular
        // https://m2.material.io/design/color/the-color-system.html#tools-for-picking-colors
        private const string COLOR_PRIMARY_LIGHT = "#ed6a5a";
        private const string COLOR_PRIMARY_DARK = "#e3847c";
        private MudThemeProvider _mudThemeProvider = null!;
        private readonly MudTheme _theme = new() {
            PaletteLight = new() {
                Primary = COLOR_PRIMARY_LIGHT,
                Secondary = "#770a00",
                Tertiary = "#5c005f",
                AppbarBackground = COLOR_PRIMARY_LIGHT
            },
            PaletteDark = new() {
                Primary = COLOR_PRIMARY_DARK,
                Secondary = "#a32116",
                Tertiary = "#960f7b",
                AppbarBackground = COLOR_PRIMARY_DARK
            }
        };
        private bool _isDarkMode;
        private bool _drawerOpen = true;

        private HubConnection? _hubConnection;
        private bool _isInitialized = false;
        private bool _connectionError = false;
        private string _statusMessage = "";

        private void DrawerToggle() => _drawerOpen = !_drawerOpen;

        protected override async Task OnInitializedAsync() {
            try {
                _statusMessage = "Connecting to local server...";
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(HostConfiguration["ApiUrl"] + HostConfiguration["DbInitializeHubPath"])
                    .Build();
                _hubConnection.On<DbInitStatus, string>("ReceiveStatus", UpdateStatus);
                await _hubConnection.StartAsync();
            } catch (HttpRequestException) {
                _connectionError = true;
                _statusMessage = "Connection error. Please reload after starting the local server.";
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                _isDarkMode = await _mudThemeProvider.GetSystemPreference();
            }
        }

        private async Task UpdateStatus(DbInitStatus status, string message) {
            switch (status) {
                case DbInitStatus.InProgress:
                    _statusMessage = message;
                    StateHasChanged();
                    break;
                case DbInitStatus.Complete:
                    _statusMessage = "Fetching data from database...";
                    StateHasChanged();
                    if (!LanguageTypeService.IsLoaded) {
                        await LanguageTypeService.Load();
                    }
                    _statusMessage = "Initialization complete";
                    StateHasChanged();
                    _isInitialized = true;
                    if (_hubConnection is not null) {
                        await _hubConnection.DisposeAsync();
                    }
                    StateHasChanged();
                    break;
            }
        }

        public async ValueTask DisposeAsync() {
            GC.SuppressFinalize(this);
            if (_hubConnection != null) {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
