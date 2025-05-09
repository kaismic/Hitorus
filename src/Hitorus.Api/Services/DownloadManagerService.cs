using Hitorus.Api.Download;
using Hitorus.Data;
using Hitorus.Data.DbContexts;
using Hitorus.Data.Entities;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace Hitorus.Api.Services {
    public class DownloadManagerService
        (
            IServiceProvider serviceProvider,
            ILogger<DownloadManagerService> logger,
            IEventBus<DownloadEventArgs> eventBus,
            IConfiguration appConfiguration,
            HttpClient httpClient
        ) : BackgroundService {
        private const int SERVER_TIME_EXCLUDE_LENGTH = 16; // length of the string "0123456789/'\r\n};"
        private readonly string _hitomiGgjsAddress = $"https://ltn.{appConfiguration["HitomiServerDomain"]}/gg.js";
        private bool _lsiInitialized = false;
        public LiveServerInfo LiveServerInfo { get; private set; } = new();

        private readonly ConcurrentDictionary<int, Downloader> _liveDownloaders = [];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            await Task.Run(() => {
                HitomiContext dbContext = new();
                DownloadConfiguration config = dbContext.DownloadConfigurations.First();
                foreach (int id in config.Downloads) {
                    GetOrCreateDownloader(id, false);
                }
                dbContext.Dispose();
            }, CancellationToken.None);
            try {
                ChannelReader<DownloadEventArgs> reader = eventBus.Subscribe();
                await foreach (DownloadEventArgs args in reader.ReadAllAsync(stoppingToken)) {
                    logger.LogInformation("Download Event Received: Ids = [{Ids}], Action = {Action}", string.Join(", ", args.GalleryIds), args.Action);
                    switch (args.Action) {
                        case DownloadAction.Create: {
                            foreach (int id in args.GalleryIds) {
                                GetOrCreateDownloader(id, true);
                            }
                            break;
                        }
                        case DownloadAction.Start: {
                            if (!_lsiInitialized) {
                                logger.LogInformation("Fetching Live Server Info...");
                                try {
                                    await UpdateLiveServerInfo();
                                    _lsiInitialized = true;
                                } catch (HttpRequestException e) {
                                    logger.LogError(e, "Failed to fetch Live Server Info.");
                                    foreach (int id in args.GalleryIds) {
                                        if (_liveDownloaders.TryGetValue(id, out Downloader? value)) {
                                            value.ChangeStatus(DownloadStatus.Failed, "Failed to fetch Live Server Info.");
                                        }
                                    }
                                    break;
                                }
                            }
                            foreach (int id in args.GalleryIds) {
                                _ = GetOrCreateDownloader(id, true).Start();
                            }
                            break;
                        }
                        case DownloadAction.Pause: {
                            foreach (int id in args.GalleryIds) {
                                if (_liveDownloaders.TryGetValue(id, out Downloader? value)) {
                                    value.Pause();
                                }
                            }
                            break;
                        }
                        case DownloadAction.Delete: {
                            foreach (int id in args.GalleryIds) {
                                if (_liveDownloaders.TryGetValue(id, out Downloader? value)) {
                                    value.Delete();
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

        private Downloader GetOrCreateDownloader(int galleryId, bool addToDb) =>
            _liveDownloaders.GetOrAdd(
                galleryId,
                (galleryId) => {
                    logger.LogInformation("{GalleryId}: Creating Downloader.", galleryId);
                    IServiceScope scope = serviceProvider.CreateScope();
                    if (addToDb) {
                        using HitomiContext dbContext = scope.ServiceProvider.GetRequiredService<HitomiContext>();
                        ICollection<int> downloads = dbContext.DownloadConfigurations.First().Downloads;
                        if (!downloads.Contains(galleryId)) {
                            downloads.Add(galleryId);
                            dbContext.SaveChanges();
                        }
                    }
                    return new(scope) {
                        GalleryId = galleryId,
                        DownloadManagerService = this
                    };
                }
            );

        public void DeleteDownloader(int id, bool startNext) {
            if (_liveDownloaders.TryRemove(id, out Downloader? downloader)) {
                downloader.Dispose();
            }
            using HitomiContext dbContext = new();
            DownloadConfiguration config = dbContext.DownloadConfigurations.First();
            if (config.Downloads.Remove(id)) {
                dbContext.SaveChanges();
            }
            if (startNext) {
                StartNext();
            }
        }

        private void StartNext() {
            using HitomiContext dbContext = new();
            DownloadConfiguration config = dbContext.DownloadConfigurations.First();
            if (!config.UseParallelDownload) {
                Downloader? firstPaused = null;
                foreach (Downloader d in _liveDownloaders.Values) {
                    if (d.Status == DownloadStatus.Downloading) {
                        return;
                    } else if (firstPaused == null && d.Status == DownloadStatus.Paused) {
                        firstPaused = d;
                    }
                }
                // no currently downloading downloads so start the first paused download
                if (firstPaused != null) {
                    _ = firstPaused.Start();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="HttpRequestException"></exception>
        /// <returns></returns>
        public async Task UpdateLiveServerInfo() {
            HttpResponseMessage response = await httpClient.GetAsync(_hitomiGgjsAddress);
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
        }
    }
}
