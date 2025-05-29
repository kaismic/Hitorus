﻿using Hitorus.Data.Entities;

namespace Hitorus.Data.DTOs;
public class DownloadConfigurationDTO
{
    public int Id { get; set; }
    public bool UseParallelDownload { get; set; }
    public int ThreadNum { get; set; }
    public ICollection<int> Downloads { get; set; } = [];
    public string PreferredFormat { get; set; } = "webp";
}
