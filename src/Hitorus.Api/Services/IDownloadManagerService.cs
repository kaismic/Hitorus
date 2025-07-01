using Hitorus.Api.Download;

namespace Hitorus.Api.Services;

public interface IDownloadManagerService {
    LiveServerInfo LiveServerInfo { get; }
    void DeleteDownloader(IDownloader downloader, bool startNext);
    void OnDownloaderIdChange(int oldId, int newId);
    Task UpdateLiveServerInfo();
}