using Blazored.LocalStorage;
using DebounceThrottle;
using Hitorus.Data;
using Hitorus.Web.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace Hitorus.Web.Services {
    public class DownloadClientManagerService(
        GalleryService galleryService,
        IConfiguration hostConfiguration,
        ISyncLocalStorageService localStorageService,
        BrowseConfigurationService browseConfigurationService,
        DownloadService downloadService,
        IJSRuntime jsRuntime,
        IStringLocalizer<DownloadClientManagerService> localizer
    ) : IAsyncDisposable, IDownloadClient {
        private HubConnection? _hubConnection;
        public Dictionary<int, DownloadModel> Downloads { get; } = [];

        public bool IsHubConnectionOpen => _hubConnection?.State == HubConnectionState.Connected;
        public Action DownloadPageStateHasChanged { get; set; } = () => { };

        private readonly DebounceDispatcher _loadGalleriesDebDispatcher = new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));

        private const int DELETE_ANIM_DURATION = 2000; // milliseconds
        private const string START_DELETE_ANIM_JS_FUNC = "startDeleteAnimation";

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
            if (Downloads.TryGetValue(galleryId, out DownloadModel? model)) {
                model.Gallery = await galleryService.GetDownloadGalleryDTO(galleryId);
                model.StateHasChanged();
                if (browseConfigurationService.BrowsePageLoaded) {
                    _loadGalleriesDebDispatcher.Debounce(browseConfigurationService.LoadGalleries);
                } else {
                    browseConfigurationService.BrowsePageRefreshQueued = true;
                }
            }
        }

        public Task ReceiveProgress(int galleryId, int progress) {
            if (Downloads.TryGetValue(galleryId, out DownloadModel? model)) {
                model.Progress = progress;
                model.StateHasChanged();
            }
            return Task.CompletedTask;
        }

        public async Task ReceiveStatus(int galleryId, DownloadStatus status) {
            if (Downloads.TryGetValue(galleryId, out DownloadModel? model)) {
                model.Status = status;
                switch (status) {
                    case DownloadStatus.Enqueued:
                        model.StatusMessage = localizer["DownloadStatus_Enqueued"];
                        break;
                    case DownloadStatus.Downloading:
                        model.StatusMessage = localizer["DownloadStatus_Downloading"];
                        break;
                    case DownloadStatus.Completed:
                        if (browseConfigurationService.BrowsePageLoaded) {
                            _loadGalleriesDebDispatcher.Debounce(browseConfigurationService.LoadGalleries);
                        } else {
                            browseConfigurationService.BrowsePageRefreshQueued = true;
                        }
                        model.StatusMessage = localizer["DownloadStatus_Completed"];
                        await jsRuntime.InvokeVoidAsync(START_DELETE_ANIM_JS_FUNC, model.ElementId, galleryId, DELETE_ANIM_DURATION);
                        _ = Task.Delay(DELETE_ANIM_DURATION).ContinueWith(_ => DeleteDownloadModel(galleryId));
                        break;
                    case DownloadStatus.Paused:
                        model.StatusMessage = localizer["DownloadStatus_Paused"];
                        break;
                    case DownloadStatus.Deleted:
                        model.StatusMessage = "";
                        await jsRuntime.InvokeVoidAsync(START_DELETE_ANIM_JS_FUNC, model.ElementId, galleryId, DELETE_ANIM_DURATION);
                        _ = Task.Delay(DELETE_ANIM_DURATION).ContinueWith(_ => DeleteDownloadModel(galleryId));
                        break;
                    case DownloadStatus.Failed:
                        throw new InvalidOperationException($"{DownloadStatus.Failed} must be handled by {nameof(ReceiveFailure)}");
                }
                model.StateHasChanged();
            }
        }

        public Task ReceiveFailure(int galleryId, string message) {
            if (Downloads.TryGetValue(galleryId, out DownloadModel? model)) {
                model.Status = DownloadStatus.Failed;
                model.StatusMessage = message;
                model.StateHasChanged();
            }
            return Task.CompletedTask;
        }

        public Task ReceiveIdChange(int oldId, int newId) {
            if (Downloads.TryGetValue(oldId, out DownloadModel? model)) {
                Downloads.Remove(oldId);
                Downloads.TryAdd(newId, model);
                model.GalleryId = newId;
                model.StateHasChanged();
            }
            return Task.CompletedTask;
        }

        private async Task OnClosed(Exception? e) {
            DownloadPageStateHasChanged();
            if (_hubConnection != null) {
                foreach (DownloadModel d in Downloads.Values) {
                    d.StatusMessage = localizer["DownloadStatus_ConnectionLost"];
                    d.Status = DownloadStatus.Failed;
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

        public void DeleteDownloadModel(int galleryId) {
            Downloads.Remove(galleryId);
            DownloadPageStateHasChanged();
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
