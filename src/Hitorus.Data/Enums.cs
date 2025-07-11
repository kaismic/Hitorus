﻿namespace Hitorus.Data {
    public enum DownloadStatus {
        Downloading,
        Queued,
        Completed,
        Paused,
        Failed,
        Deleted
    }

    public enum DownloadAction {
        GalleryInfoOnly,
        Queue,
        Start,
        Pause,
        Delete
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
        Id,
        Title,
        UploadTime,
        LastDownloadTime,
        UserDefinedOrder
    }
}