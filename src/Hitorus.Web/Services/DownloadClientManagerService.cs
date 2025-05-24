using Blazored.LocalStorage;
using Hitorus.Data;
using Hitorus.Web.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace Hitorus.Web.Services {
    public class DownloadClientManagerService : IAsyncDisposable {
        private readonly GalleryService _galleryService;
        private readonly IConfiguration _hostConfiguration;
        private readonly ISyncLocalStorageService _localStorageService;
        private readonly DownloadConfigurationService _downloadConfigurationService;
        private readonly DownloadService _downloadService;
        private readonly IJSRuntime _jsRuntime;
        private readonly IStringLocalizer<DownloadClientManagerService> _localizer;

        private HubConnection? _hubConnection;
        public Dictionary<int, DownloadModel> Downloads { get; } = [];

        public bool IsHubConnectionOpen => _hubConnection?.State == HubConnectionState.Connected;
        public Action DownloadPageStateHasChanged { get; set; } = () => { };
        private readonly DotNetObjectReference<DownloadClientManagerService> _dotNetObjectRef;

        public DownloadClientManagerService(
            GalleryService galleryService,
            IConfiguration hostConfiguration,
            ISyncLocalStorageService localStorageService,
            DownloadConfigurationService downloadConfigurationService,
            DownloadService downloadService,
            IJSRuntime jsRuntime,
            IStringLocalizer<DownloadClientManagerService> localizer
        ) {
            _galleryService = galleryService;
            _hostConfiguration = hostConfiguration;
            _localStorageService = localStorageService;
            _downloadConfigurationService = downloadConfigurationService;
            _downloadService = downloadService;
            _jsRuntime = jsRuntime;
            _localizer = localizer;
            _dotNetObjectRef = DotNetObjectReference.Create(this);
        }

        public void OpenHubConnection() {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Utilities.GetServiceBaseUri(_hostConfiguration, _localStorageService, "DownloadHubPath"))
                .Build();
            _hubConnection.On<IEnumerable<int>>("ReceiveSavedDownloads", OnReceiveSavedDownloads);
            _hubConnection.On<int>("ReceiveGalleryAvailable", OnReceiveGalleryAvailable);
            _hubConnection.On<int, int>("ReceiveProgress", OnReceiveProgress);
            _hubConnection.On<int, DownloadStatus>("ReceiveStatus", OnReceiveStatus);
            _hubConnection.On<int, string>("ReceiveFailure", OnReceiveFailure);
            _hubConnection.On<int, int>("ReceiveIdChange", OnReceiveIdChange);
            _hubConnection.Closed += OnClosed;
            _hubConnection.StartAsync();
        }

        private void OnReceiveSavedDownloads(IEnumerable<int> galleryIds) {
            foreach (int id in galleryIds) {
                Downloads.Add(id, new() { GalleryId = id });
            }
            DownloadPageStateHasChanged();
        }

        private async Task OnReceiveGalleryAvailable(int galleryId) {
            if (Downloads.TryGetValue(galleryId, out DownloadModel? model)) {
                model.Gallery = await _galleryService.GetDownloadGalleryDTO(galleryId);
                model.StateHasChanged();
            }
        }

        private void OnReceiveProgress(int galleryId, int progress) {
            if (Downloads.TryGetValue(galleryId, out DownloadModel? model)) {
                model.Progress = progress;
                model.StateHasChanged();
            }
        }

        private async Task OnReceiveStatus(int galleryId, DownloadStatus status) {
            if (Downloads.TryGetValue(galleryId, out DownloadModel? model)) {
                model.Status = status;
                switch (status) {
                    case DownloadStatus.Downloading:
                        model.StatusMessage = _localizer["DownloadStatus_Downloading"];
                        model.WaitingResponse = false;
                        break;
                    case DownloadStatus.Completed:
                        model.StatusMessage = _localizer["DownloadStatus_Completed"];
                        await _jsRuntime.InvokeVoidAsync("startDeleteAnimation", model.ElementId, galleryId, _dotNetObjectRef);
                        break;
                    case DownloadStatus.Paused:
                        model.StatusMessage = _localizer["DownloadStatus_Paused"];
                        model.WaitingResponse = false;
                        break;
                    case DownloadStatus.Deleted:
                        model.StatusMessage = "";
                        await _jsRuntime.InvokeVoidAsync("startDeleteAnimation", model.ElementId, galleryId, _dotNetObjectRef);
                        break;
                    case DownloadStatus.Failed:
                        throw new InvalidOperationException($"{DownloadStatus.Failed} must be handled by {nameof(OnReceiveFailure)}");
                }
                model.StateHasChanged();
            }
        }

        private void OnReceiveFailure(int galleryId, string message) {
            if (Downloads.TryGetValue(galleryId, out DownloadModel? model)) {
                model.WaitingResponse = false;
                model.Status = DownloadStatus.Failed;
                model.StatusMessage = message;
                model.StateHasChanged();
            }
        }

        private void OnReceiveIdChange(int oldId, int newId) {
            if (Downloads.TryGetValue(oldId, out DownloadModel? model)) {
                Downloads.Remove(oldId);
                Downloads.TryAdd(newId, model);
                model.GalleryId = newId;
                model.StateHasChanged();
            }
        }

        private async Task OnClosed(Exception? e) {
            if (_hubConnection != null) {
                foreach (DownloadModel d in Downloads.Values) {
                    d.StatusMessage = _localizer["DownloadStatus_ConnectionLost"];
                    d.Status = DownloadStatus.Failed;
                }
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }

        public async Task AddDownloads(IEnumerable<int> galleryIds) {
            HashSet<int> ids = [.. galleryIds];
            foreach (int id in ids) {
                Downloads.TryAdd(id, new() {
                    GalleryId = id,
                    StatusMessage = _localizer["DownloadStatus_Starting"]
                });
            }
            if (_downloadConfigurationService.Config.UseParallelDownload) {
                await _downloadService.StartDownloaders(ids);
            } else {
                await _downloadService.CreateDownloaders(ids);
            }
            if (!_downloadConfigurationService.Config.UseParallelDownload) {
                DownloadModel? firstPaused = null;
                foreach (DownloadModel d in Downloads.Values) {
                    if (d.Status == DownloadStatus.Downloading) {
                        return;
                    } else if (firstPaused == null && d.Status == DownloadStatus.Paused && ids.Contains(d.GalleryId)) {
                        firstPaused = d;
                    }
                }
                // no currently downloading downloads so start the first paused download
                if (firstPaused != null) {
                    await _downloadService.StartDownloaders([firstPaused.GalleryId]);
                }
            }
        }

        public async Task StartAllDownloads() {
            List<int> ids = new(Downloads.Count);
            foreach (DownloadModel d in Downloads.Values) {
                if (d.Status is DownloadStatus.Paused or DownloadStatus.Failed) {
                    ids.Add(d.GalleryId);
                }
            }
            await _downloadService.StartDownloaders(ids);
        }

        public async Task PauseAllDownloads() {
            List<int> ids = new(Downloads.Count);
            foreach (DownloadModel d in Downloads.Values) {
                if (d.Status is DownloadStatus.Downloading) {
                    ids.Add(d.GalleryId);
                }
            }
            await _downloadService.PauseDownloaders(ids);
        }

        public async Task DeleteAllDownloads() {
            List<int> ids = new(Downloads.Count);
            foreach (DownloadModel d in Downloads.Values) {
                if (d.Status is not DownloadStatus.Completed or DownloadStatus.Deleted) {
                    ids.Add(d.GalleryId);
                }
            }
            await _downloadService.DeleteDownloaders(ids);
        }

        [JSInvokable]
        public void OnDeleteAnimationFinished(int galleryId) {
            Downloads.Remove(galleryId);
            DownloadPageStateHasChanged();
        }

        public async ValueTask DisposeAsync() {
            GC.SuppressFinalize(this);
            if (_hubConnection != null) {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
            _dotNetObjectRef.Dispose();
        }
    }
}
