using Hitorus.Data;
using Hitorus.Data.DTOs;

namespace Hitorus.Web.Models {
    public class DownloadModel {
        public string ElementId => "download-item-" + GalleryId;
        public required int GalleryId { get; init; }
        public DownloadGalleryDTO? Gallery { get; set; }
        public DownloadStatus Status { get; set; } = DownloadStatus.Paused;
        public string StatusMessage { get; set; } = "";
        public int Progress { get; set; }
        public bool WaitingResponse { get; set; } = false;
        public Action StateHasChanged { get; set; } = () => { };
    }
}