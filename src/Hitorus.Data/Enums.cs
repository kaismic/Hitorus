namespace Hitorus.Data {
    public enum DbInitStatus {
        InProgress,
        Complete
    }

    public enum DownloadStatus {
        Downloading,
        Completed,
        Paused,
        Failed,
        Deleted
    }

    public enum DownloadAction {
        Create,
        Start,
        Pause,
        Delete,
        Import,
        Complete
    }

    public enum ViewMode {
        Default = 0,
        Scroll = 1
    }

    public enum ImageLayoutMode {
        Automatic,
        Fixed
    }

    public enum ViewDirection {
        LTR,
        RTL
    }

    public enum AutoScrollMode {
        Continuous = 0,
        ByPage = 1,
    }

    public enum FitMode {
        Automatic,
        Horizontal,
        Vertical
    }

    public enum GalleryProperty {
        Id, Title, UploadTime, LastDownloadTime
    }
}