using Blazored.LocalStorage;
using DebounceThrottle;
using Hitorus.Data;
using Hitorus.Web.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Localization;

namespace Hitorus.Web.Services {
    public class DownloadClientManagerService(
        GalleryService galleryService,
        IConfiguration hostConfiguration,
        ISyncLocalStorageService localStorageService,
        BrowseConfigurationService browseConfigurationService,
        DownloadService downloadService,
        IStringLocalizer<DownloadClientManagerService> localizer
    ) : IAsyncDisposable, IDownloadClient {
        private HubConnection? _hubConnection;
        public Dictionary<int, DownloadItemViewModel> Downloads { get; } = [];

        public bool IsHubConnectionOpen => _hubConnection?.State == HubConnectionState.Connected;
        public Action DownloadPageStateHasChanged { get; set; } = () => { };

        private readonly DebounceDispatcher _loadGalleriesDebDispatcher = new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));

        public void OpenHubConnection() {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "DownloadHubPath"))
                .Build();
            _hubConnection.On<IEnumerable<int>>(nameof(ReceiveCreateDownloads), ReceiveCreateDownloads);
            _hubConnection.On<int>(nameof(ReceiveGalleryAvailable), ReceiveGalleryAvailable);
            _hubConnection.On<int, int>(nameof(ReceiveProgress), ReceiveProgress);
            _hubConnection.On<int, DownloadStatus>(nameof(ReceiveStatus), ReceiveStatus);
            _hubConnection.On<int, string>(nameof(ReceiveFailure), ReceiveFailure);
            _hubConnection.On<int, int>(nameof(ReceiveIdChange), ReceiveIdChange);
            _hubConnection.Closed += OnClosed;
            _hubConnection.StartAsync();
        }

        public Task ReceiveCreateDownloads(IEnumerable<int> galleryIds) {
            foreach (int id in galleryIds) {
                if (!Downloads.ContainsKey(id)) {
                    Downloads.Add(id, new() { GalleryId = id });
                }
            }
            DownloadPageStateHasChanged();
            return Task.CompletedTask;
        }

        public async Task ReceiveGalleryAvailable(int galleryId) {
            if (Downloads.TryGetValue(galleryId, out DownloadItemViewModel? vm)) {
                vm.Gallery = await galleryService.GetDownloadGalleryDTO(galleryId);
                // TODO test if component is rendered and displays gallery title without StateHasChanged
                if (browseConfigurationService.BrowsePageLoaded) {
                    _loadGalleriesDebDispatcher.Debounce(browseConfigurationService.LoadGalleries);
                } else {
                    browseConfigurationService.BrowsePageRefreshQueued = true;
                }
            }
        }

        public Task ReceiveProgress(int galleryId, int progress) {
            if (Downloads.TryGetValue(galleryId, out DownloadItemViewModel? vm)) {
                vm.Progress = progress;
            }
            return Task.CompletedTask;
        }

        public Task ReceiveStatus(int galleryId, DownloadStatus status) {
            if (Downloads.TryGetValue(galleryId, out DownloadItemViewModel? vm)) {
                vm.Status = status;
                switch (status) {
                    case DownloadStatus.Paused or DownloadStatus.Downloading:
                        vm.WaitingResponse = false;
                        DownloadPageStateHasChanged();
                        break;
                    case DownloadStatus.Completed or DownloadStatus.Deleted:
                        if (status == DownloadStatus.Completed) {
                            if (browseConfigurationService.BrowsePageLoaded) {
                                _loadGalleriesDebDispatcher.Debounce(browseConfigurationService.LoadGalleries);
                            } else {
                                browseConfigurationService.BrowsePageRefreshQueued = true;
                            }
                        }
                        _ = Task.Run(async () => {
                            await vm.BeginDeleteSelf();
                            Downloads.Remove(galleryId);
                            DownloadPageStateHasChanged();
                        });
                        break;
                    case DownloadStatus.Failed:
                        throw new InvalidOperationException($"{DownloadStatus.Failed} must be handled by {nameof(ReceiveFailure)}");
                }
            }
            return Task.CompletedTask;
        }

        public Task ReceiveFailure(int galleryId, string message) {
            if (Downloads.TryGetValue(galleryId, out DownloadItemViewModel? vm)) {
                vm.Status = DownloadStatus.Failed;
                vm.ErrorMessage = message;
            }
            return Task.CompletedTask;
        }

        public Task ReceiveIdChange(int oldId, int newId) {
            if (Downloads.TryGetValue(oldId, out DownloadItemViewModel? vm)) {
                Downloads.Remove(oldId);
                vm.GalleryId = newId;
                Downloads.TryAdd(newId, vm);
            }
            return Task.CompletedTask;
        }

        private async Task OnClosed(Exception? e) {
            DownloadPageStateHasChanged();
            if (_hubConnection != null) {
                foreach (var vm in Downloads.Values) {
                    vm.Status = DownloadStatus.Failed;
                    vm.ErrorMessage = localizer["DownloadStatus_ConnectionLost"];
                }
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }

        public async Task SendPauseAllDownloads() {
            IEnumerable<int> ids = Downloads.Values
                .Where(d => d.Status is DownloadStatus.Downloading)
                .Select(d => d.GalleryId);
            await downloadService.SendAction(DownloadAction.Pause, ids);
        }

        public async Task SendDeleteAllDownloads() {
            IEnumerable<int> ids = Downloads.Values.Select(d => d.GalleryId);
            await downloadService.SendAction(DownloadAction.Delete, ids);
        }

        public async Task HandleDownloadItemActionRequest(int galleryId) {
            if (Downloads.TryGetValue(galleryId, out DownloadItemViewModel? vm)) {
                vm.WaitingResponse = true;
                bool success = false;
                switch (vm.Status) {
                    case DownloadStatus.Downloading:
                        success = await downloadService.SendAction(DownloadAction.Pause, [vm.GalleryId]);
                        break;
                    case DownloadStatus.Paused or DownloadStatus.Failed or DownloadStatus.Enqueued:
                        success = await downloadService.SendAction(DownloadAction.Start, [vm.GalleryId]);
                        break;
                    case DownloadStatus.Completed or DownloadStatus.Deleted:
                        throw new InvalidOperationException("Action button should not be clickable.");
                }
                if (!success) {
                    vm.WaitingResponse = false;
                }
            }
        }

        public async Task HandleDownloadItemDeleteRequest(int galleryId) {
            if (Downloads.TryGetValue(galleryId, out DownloadItemViewModel? vm)) {
                vm.WaitingResponse = true;
                bool success = await downloadService.SendAction(DownloadAction.Delete, [vm.GalleryId]);
                if (!success) {
                    vm.WaitingResponse = false;
                }
            }
        }

        public async ValueTask DisposeAsync() {
            GC.SuppressFinalize(this);
            if (_hubConnection != null) {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
            _loadGalleriesDebDispatcher.Dispose();
        }
    }
}
