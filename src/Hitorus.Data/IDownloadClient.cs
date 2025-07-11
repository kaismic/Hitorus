﻿namespace Hitorus.Data {
    public interface IDownloadClient {
        Task ReceiveSavedDownloads(IEnumerable<int> galleryIds);
        Task ReceiveCreateDownloads(IEnumerable<int> galleryIds);
        Task ReceiveGalleryAvailable(int galleryId);
        Task ReceiveProgress(int galleryId, int progress);
        Task ReceiveStatus(int galleryId, DownloadStatus status);
        Task ReceiveFailure(int galleryId, string message);
        Task ReceiveIdChange(int oldId, int newId);
    }
}
