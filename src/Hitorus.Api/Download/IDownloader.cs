using Hitorus.Data;

namespace Hitorus.Api.Download {
    public interface IDownloader : IDisposable {
        DownloadStatus Status { get; }
        Task Start();
        void Pause();
        void Delete();
        void Fail(string message);
    }
}
