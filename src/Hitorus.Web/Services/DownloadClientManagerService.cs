using Blazored.LocalStorage;
using DebounceThrottle;
using Hitorus.Data;
using Hitorus.Data.Entities;
using Hitorus.Web.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace Hitorus.Web.Services {
    public class DownloadClientManagerService(
        GalleryService galleryService,
        IConfiguration hostConfiguration,
        ISyncLocalStorageService localStorageService,
        DownloadConfigurationService downloadConfigurationService,
        BrowseConfigurationService browseConfigurationService,
        DownloadService downloadService,
        IJSRuntime jsRuntime,
        IStringLocalizer<DownloadClientManagerService> localizer
    ) : IAsyncDisposable {
        private HubConnection? _hubConnection;
        public Dictionary<int, DownloadModel> Downloads { get; } = [];

        public bool IsHubConnectionOpen => _hubConnection?.State == HubConnectionState.Connected;
        public Action DownloadPageStateHasChanged { get; set; } = () => { };

        private readonly DebounceDispatcher _loadGalleriesDebDispatcher = new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3));

        private const int DELETE_ANIM_DURATION = 2000; // milliseconds

        public void OpenHubConnection() {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "DownloadHubPath"))
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
                model.Gallery = await galleryService.GetDownloadGalleryDTO(galleryId);
                model.StateHasChanged();
                if (browseConfigurationService.BrowsePageLoaded) {
                    _loadGalleriesDebDispatcher.Debounce(browseConfigurationService.LoadGalleries);
                } else {
                    browseConfigurationService.BrowsePageRefreshQueued = true;
                }
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
                        model.StatusMessage = localizer["DownloadStatus_Downloading"];
                        model.WaitingResponse = false;
                        break;
                    case DownloadStatus.Completed:
                        if (browseConfigurationService.BrowsePageLoaded) {
                            _loadGalleriesDebDispatcher.Debounce(browseConfigurationService.LoadGalleries);
                        } else {
                            browseConfigurationService.BrowsePageRefreshQueued = true;
                        }
                        model.StatusMessage = localizer["DownloadStatus_Completed"];
                        await jsRuntime.InvokeVoidAsync("startDeleteAnimation", model.ElementId, galleryId, DELETE_ANIM_DURATION);
                        _ = Task.Delay(DELETE_ANIM_DURATION).ContinueWith(_ => DeleteDownload(galleryId));
                        break;
                    case DownloadStatus.Paused:
                        model.StatusMessage = localizer["DownloadStatus_Paused"];
                        model.WaitingResponse = false;
                        break;
                    case DownloadStatus.Deleted:
                        model.StatusMessage = "";
                        await jsRuntime.InvokeVoidAsync("startDeleteAnimation", model.ElementId, galleryId, DELETE_ANIM_DURATION);
                        _ = Task.Delay(DELETE_ANIM_DURATION).ContinueWith(_ => DeleteDownload(galleryId));
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
                    d.StatusMessage = localizer["DownloadStatus_ConnectionLost"];
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
                    StatusMessage = localizer["DownloadStatus_Starting"]
                });
            }
            if (downloadConfigurationService.Config.UseParallelDownload) {
                await downloadService.StartDownloaders(ids);
            } else {
                await downloadService.CreateDownloaders(ids);
            }
            if (!downloadConfigurationService.Config.UseParallelDownload) {
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
                    await downloadService.StartDownloaders([firstPaused.GalleryId]);
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
            await downloadService.StartDownloaders(ids);
        }

        public async Task PauseAllDownloads() {
            List<int> ids = new(Downloads.Count);
            foreach (DownloadModel d in Downloads.Values) {
                if (d.Status is DownloadStatus.Downloading) {
                    ids.Add(d.GalleryId);
                }
            }
            await downloadService.PauseDownloaders(ids);
        }

        public async Task DeleteAllDownloads() {
            List<int> ids = new(Downloads.Count);
            foreach (DownloadModel d in Downloads.Values) {
                if (d.Status is not DownloadStatus.Completed or DownloadStatus.Deleted) {
                    ids.Add(d.GalleryId);
                }
            }
            await downloadService.DeleteDownloaders(ids);
            _ = Task.Delay(DELETE_ANIM_DURATION).ContinueWith(_ => {
                // this is not really necessary if the server sends the delete event correctly
                // but it is a good fallback in case the server does not send the delete event
                // e.g. the downloads might have beeen already deleted so the server does not send the delete event
                foreach (int id in ids) {
                    Downloads.Remove(id);
                }
            });
        }

        public void DeleteDownload(int galleryId) {
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
