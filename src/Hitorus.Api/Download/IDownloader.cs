using Hitorus.Data;

namespace Hitorus.Api.Download {
    public interface IDownloader : IDisposable {
        bool GalleryInfoOnly { get; }
        int GalleryId { get; }
        DownloadStatus Status { get; }
        LiveServerInfo LiveServerInfo { set; }
        void ChangeStatus(DownloadStatus status, string? message = null);
        Task Start();
        event Action<int> DownloadCompleted;
    }
}
