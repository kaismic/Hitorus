using Hitorus.Api.Download;

namespace Hitorus.Api.Services;

public interface IDownloadManagerService {
    LiveServerInfo LiveServerInfo { get; }

    void DeleteDownloader(int id, bool startNext);
    void OnDownloaderIdChange(int oldId, int newId);
    Task UpdateLiveServerInfo();
    IDownloader GetOrCreateDownloader(int galleryId, bool addToDb);
}