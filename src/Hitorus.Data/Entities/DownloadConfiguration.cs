using Hitorus.Data.DTOs;

namespace Hitorus.Data.Entities;

public class DownloadConfiguration
{
    public virtual int Id { get; set; }
    public virtual int MaxConcurrentDownloadCount { get; set; }
    public virtual int DownloadThreadCount { get; set; }
    public virtual ICollection<int> SavedDownloads { get; set; } = [];
    public virtual string PreferredFormat { get; set; } = "webp";

    public DownloadConfigurationDTO ToDTO() => new() {
        Id = Id,
        MaxConcurrentDownloadCount = MaxConcurrentDownloadCount,
        DownloadThreadCount = DownloadThreadCount,
        SavedDownloads = SavedDownloads,
        PreferredFormat = PreferredFormat
    };
}
