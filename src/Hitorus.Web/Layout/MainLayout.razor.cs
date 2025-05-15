using Hitorus.Data;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Hitorus.Web.Layout {
    public partial class MainLayout : LayoutComponentBase, IAsyncDisposable {
        [Inject] LanguageTypeService LanguageTypeService { get; set; } = default!;
        [Inject] IConfiguration HostConfiguration { get; set; } = default!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;

        private MudThemeProvider _mudThemeProvider = null!;

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
