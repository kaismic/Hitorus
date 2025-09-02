using Hitorus.Data;
using Hitorus.Data.DTOs;

namespace Hitorus.Web.ViewModels {
    public class DownloadItemViewModel {
        public const int DELETE_ANIM_DURATION = 2000; // milliseconds
        public Func<string, int, ValueTask>? StartDeleteAnimation;
        public string ElementId => "download-item-" + GalleryId;
        public required int GalleryId { get; set; }
        public DownloadGalleryDTO? Gallery { get; set; }
        public DownloadStatus Status { get; set; } = DownloadStatus.Paused;
        // TODO localize
        public string StatusMessage => Status switch {
            DownloadStatus.Enqueued => "Enqueued",
            DownloadStatus.Downloading => "Downloading",
            DownloadStatus.Paused => Gallery == null ? "" : "Download Paused",
            DownloadStatus.Completed => "Download Completed",
            DownloadStatus.Failed => "Download Failed",
            DownloadStatus.Deleted => "",
            _ => throw new NotImplementedException()
        };
        public string? ErrorMessage { get; set; }
        public int Progress { get; set; }
        public bool WaitingResponse { get; set; } = false;
        public bool Visible { get; private set; } = true;

        public async Task BeginDeleteSelf() {
            WaitingResponse = true;
            if (StartDeleteAnimation != null) {
                await StartDeleteAnimation.Invoke(ElementId, DELETE_ANIM_DURATION);
                await Task.Delay(DELETE_ANIM_DURATION);
            }
            Visible = false;
        }
    }
}
