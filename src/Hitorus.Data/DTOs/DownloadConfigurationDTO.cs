namespace Hitorus.Data.DTOs;
public class DownloadConfigurationDTO
{
    public int Id { get; set; }
    public virtual int MaxConcurrentDownloadCount { get; set; }
    public virtual int DownloadThreadCount { get; set; }
    public virtual ICollection<int> SavedDownloads { get; set; } = [];
    public string PreferredFormat { get; set; } = "webp";
}
