using Hitorus.Data.DTOs;

namespace Hitorus.Data.Entities;

public class DownloadConfiguration
{
    public virtual int Id { get; set; }
    public virtual bool UseParallelDownload { get; set; }
    public virtual int ThreadNum { get; set; }
    public virtual ICollection<int> Downloads { get; set; } = [];
    public virtual string PreferredFormat { get; set; } = "webp";

    public DownloadConfigurationDTO ToDTO() => new() {
        Id = Id,
        UseParallelDownload = UseParallelDownload,
        ThreadNum = ThreadNum,
        Downloads = Downloads,
        PreferredFormat = PreferredFormat
    };
}
