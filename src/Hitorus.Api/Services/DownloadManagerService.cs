using Hitorus.Api.Download;
using Hitorus.Data;
using Hitorus.Data.DbContexts;
using Hitorus.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace Hitorus.Api.Services;
public class DownloadManagerService(
        IServiceProvider serviceProvider,
        ILogger<DownloadManagerService> logger,
        IEventBus<DownloadEventArgs> eventBus,
        IConfiguration appConfiguration,
        IDbContextFactory<HitomiContext> dbContextFactory,
        IHttpClientFactory httpClientFactory
    ) : BackgroundService, IDownloadManagerService {
    private const int SERVER_TIME_EXCLUDE_LENGTH = 16; // length of the string "0123456789/'\r\n};"
    private readonly string _hitomiGgjsAddress = $"https://ltn.{appConfiguration["HitomiServerDomain"]}/gg.js";
    public LiveServerInfo LiveServerInfo { get; private set; } = new();

    private readonly ConcurrentDictionary<int, IDownloader> _liveDownloaders = [];
    private readonly LinkedList<IDownloader> _downloaderQueue = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        // Restore saved downloads from the database
        await Task.Run(() => {
            using HitomiContext dbContext = dbContextFactory.CreateDbContext();
            DownloadConfiguration config = dbContext.DownloadConfigurations.AsNoTracking().First();
            foreach (int id in config.SavedDownloads) {
                _liveDownloaders.TryAdd(id, CreateDownloader(id, false));
            }
        }, CancellationToken.None);

        // Fetch initial Live Server Info
        logger.LogInformation("Fetching Live Server Info...");
        try {
            await UpdateLiveServerInfo();
        } catch (HttpRequestException e) {
            logger.LogError(e, "Failed to fetch Live Server Info.");
        }

        try {
            ChannelReader<DownloadEventArgs> reader = eventBus.Subscribe();
            await foreach (DownloadEventArgs args in reader.ReadAllAsync(stoppingToken)) {
                logger.LogInformation("Download event received: Ids = [{Ids}], Action = {Action}", string.Join(", ", args.GalleryIds), args.Action);
                switch (args.Action) {
                    case DownloadAction.GalleryInfoOnly: {
                        using HitomiContext dbContext = dbContextFactory.CreateDbContext();
                        foreach (int id in args.GalleryIds) {
                            if (dbContext.Galleries.Any(g => g.Id == id) || _liveDownloaders.ContainsKey(id)) {
                                continue;
                            }
                            IDownloader downloader = CreateDownloader(id, true);
                            _liveDownloaders.TryAdd(id, downloader);
                            _ = downloader.Start();
                        }
                        break;
                    }
                    case DownloadAction.Queue: {
                        using HitomiContext dbContext = dbContextFactory.CreateDbContext();
                        foreach (int id in args.GalleryIds) {
                            if (_downloaderQueue.Any(d => d.GalleryId == id) || _liveDownloaders.ContainsKey(id)) {
                                continue;
                            }
                            IDownloader downloader = CreateDownloader(id, false);
                            _downloaderQueue.AddFirst(downloader);
                            downloader.ChangeStatus(DownloadStatus.Queued);
                        }
                        break;
                    }
                    case DownloadAction.Start: {
                        foreach (int id in args.GalleryIds) {
                            if (_liveDownloaders.TryGetValue(id, out IDownloader? downloader)) {
                                _ = downloader.Start();
                                continue;
                            }
                            foreach (IDownloader d in _downloaderQueue) {
                                if (d.GalleryId == id) {
                                    _downloaderQueue.Remove(d);
                                    _liveDownloaders.TryAdd(id, d);
                                    _ = d.Start();
                                    break;
                                }
                            }
                        }
                        break;
                    }
                    case DownloadAction.Pause: {
                        foreach (int id in args.GalleryIds) {
                            if (_liveDownloaders.TryGetValue(id, out IDownloader? value)) {
                                value.ChangeStatus(DownloadStatus.Paused);
                            }
                        }
                        break;
                    }
                    case DownloadAction.Delete: {
                        foreach (int id in args.GalleryIds) {
                            if (_liveDownloaders.TryGetValue(id, out IDownloader? value)) {
                                value.ChangeStatus(DownloadStatus.Deleted);
                                DeleteDownloader(value, false);
                            }
                        }
                        break;
                    }
                }
            }
        } catch (OperationCanceledException) {
        } catch (Exception e) {
            logger.LogError(e, "");
        }
    }

    public IDownloader CreateDownloader(int galleryId, bool galleryInfoOnly) {
        Downloader downloader = new(serviceProvider.CreateScope()) {
            GalleryId = galleryId,
            GalleryInfoOnly = galleryInfoOnly,
            LiveServerInfo = LiveServerInfo,
            OnIdChange = OnDownloaderIdChange,
            UpdateLiveServerInfo = UpdateLiveServerInfo,
        };
        downloader.DownloadCompleted += OnDownloadCompleted;
        return downloader;
    }

    public void DeleteDownloader(IDownloader downloader, bool completed) {
        using HitomiContext dbContext = dbContextFactory.CreateDbContext();
        DownloadConfiguration config = dbContext.DownloadConfigurations.First();
        if (config.SavedDownloads.Remove(downloader.GalleryId)) {
            dbContext.SaveChanges();
        }
        if (!_liveDownloaders.TryRemove(downloader.GalleryId, out _)) {
            _downloaderQueue.Remove(downloader);
        }
        downloader.Dispose();
        if (completed) {
            DequeueDownloaders();
        }
    }

    private void OnDownloadCompleted(int galleryId) {
        if (_liveDownloaders.TryGetValue(galleryId, out IDownloader? value)) {
            value.DownloadCompleted -= OnDownloadCompleted;
            DeleteDownloader(value, true);
        }
    }

    public void OnDownloaderIdChange(int oldId, int newId) {
        logger.LogInformation("Downloader Id changed: Old = {old}, New = {new}", oldId, newId);
        if (_liveDownloaders.TryRemove(oldId, out IDownloader? downloader)) {
            using HitomiContext dbContext = dbContextFactory.CreateDbContext();
            DownloadConfiguration config = dbContext.DownloadConfigurations.First();
            config.SavedDownloads.Remove(oldId);
            if (_liveDownloaders.TryAdd(newId, downloader)) {
                if (!config.SavedDownloads.Contains(newId)) {
                    config.SavedDownloads.Add(newId);
                }
            } else {
                downloader.Dispose();
            }
            dbContext.SaveChanges();
        }
    }

    private void DequeueDownloaders() {
        using HitomiContext dbContext = dbContextFactory.CreateDbContext();
        DownloadConfiguration config = dbContext.DownloadConfigurations.AsNoTracking().First();
        while (_downloaderQueue.Count > 0) {
            int activeDownloaders = _liveDownloaders.Values.Count(d => d.Status == DownloadStatus.Downloading);
            if (activeDownloaders >= config.MaxConcurrentDownloadCount) {
                break;
            }
            LinkedListNode<IDownloader>? next = _downloaderQueue.First;
            if (next != null && !_liveDownloaders.ContainsKey(next.Value.GalleryId)) {
                IDownloader d = next.Value;
                _downloaderQueue.RemoveFirst();
                _liveDownloaders.TryAdd(d.GalleryId, d);
                _ = d.Start();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="HttpRequestException"></exception>
    /// <returns></returns>
    public async Task UpdateLiveServerInfo() {
        HttpResponseMessage response = await httpClientFactory.CreateClient().GetAsync(_hitomiGgjsAddress);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();

        string serverTime = content.Substring(content.Length - SERVER_TIME_EXCLUDE_LENGTH, 10);
        string selectionSetPat = @"case (\d+)";
        MatchCollection matches = Regex.Matches(content, selectionSetPat);
        HashSet<string> subdomainSelectionSet = [.. matches.Select(match => match.Groups[1].Value)];

        string orderPat = @"var [a-z] = (\d);";
        Match match = Regex.Match(content, orderPat);
        LiveServerInfo = new() {
            ServerTime = int.Parse(serverTime),
            SubdomainSelectionSet = subdomainSelectionSet,
            IsContains = match.Groups[1].Value == "0"
        };
        foreach (IDownloader d in _liveDownloaders.Values) {
            d.LiveServerInfo = LiveServerInfo;
        }
    }
}
