using Hitorus.Api.Hubs;
using Hitorus.Api.Utilities;
using Hitorus.Data;
using Hitorus.Data.DbContexts;
using Hitorus.Data.Entities;
using Microsoft.AspNetCore.SignalR;

namespace Hitorus.Api.Services {
    public class DbInitializeService(IHubContext<DbInitializeHub, IDbStatusClient> hubContext) : BackgroundService {
        private const string DB_INIT_FLAG_PATH = "db-init-flag.flag";
        private static readonly string[] ALPHABETS_WITH_123 =
            ["123", .. Enumerable.Range('a', 26).Select(intValue => Convert.ToChar(intValue).ToString())];

        private const string DB_RES_ROOT_DIR = "DatabaseResources";
        private static readonly string DELIMITER_FILE_PATH = Path.Combine(
            DB_RES_ROOT_DIR,
            "delimiter.txt"
        );
        private static readonly string LANGUAGES_FILE_PATH = Path.Combine(
            DB_RES_ROOT_DIR,
            "languages.txt"
        );
        private static readonly string TYPES_FILE_PATH = Path.Combine(
            DB_RES_ROOT_DIR,
            "types.txt"
        );
        private static readonly Dictionary<TagCategory, string> CATEGORY_DIR_DICT = new() {
            { TagCategory.Tag, "Tags" },
            { TagCategory.Male, "Males" },
            { TagCategory.Female, "Females" },
            { TagCategory.Artist, "Artists" },
            { TagCategory.Group, "Groups" },
            { TagCategory.Character, "Characters" },
            { TagCategory.Series, "Series" }
        };

        public static bool IsInitialized { get; private set; } = false;

        protected override Task ExecuteAsync(CancellationToken stoppingToken) {
            bool flagExists = File.Exists(DB_INIT_FLAG_PATH);
            if (!flagExists) {
                using HitomiContext context = new();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                Console.WriteLine("\n--- Starting database initialization ---\n");
                AddDefaultDataAsync(context);
                Console.WriteLine("\n--- Database initialization complete ---\n");
            }
            return CompleteInitialization(flagExists);
        }

        private async Task CompleteInitialization(bool fileExists) {
            if (!fileExists) {
                await File.WriteAllTextAsync(DB_INIT_FLAG_PATH, "Delete this file to re-initialize database.");
            }
            IsInitialized = true;
            await hubContext.Clients.All.ReceiveStatus(DbInitStatus.Complete, "");
        }

        private const int MAX_DESC_TEXT_LENGTH = 40;
        private static readonly ProgressBar _progressBar = new(10);
        private static readonly int _totalLeftAlignment = MAX_DESC_TEXT_LENGTH + _progressBar.TotalLength;

        private void AddDefaultDataAsync(HitomiContext context) {
            hubContext.Clients.All.ReceiveStatus(DbInitStatus.InProgress, "Adding tags...");
            string delimiter = File.ReadAllText(DELIMITER_FILE_PATH);
            foreach (TagCategory category in Tag.TAG_CATEGORIES) {
                Console.Write("{0,-" + MAX_DESC_TEXT_LENGTH + "}", $"Adding {category} tags... ");
                int progress = 0;
                string categoryStr = CATEGORY_DIR_DICT[category];
                string dir = Path.Combine(DB_RES_ROOT_DIR, categoryStr);
                foreach (string alphanumStr in ALPHABETS_WITH_123) {
                    string path = Path.Combine(dir, $"{categoryStr.ToLower()}-{alphanumStr}.txt");
                    string[] tagInfoStrs = File.ReadAllLines(path);
                    context.Tags.AddRange(tagInfoStrs.Select(
                        tagInfoStr => {
                            string[] tagInfoArr = tagInfoStr.Split(delimiter);
                            return new Tag() {
                                Category = category,
                                Value = tagInfoArr[0],
                                GalleryCount = int.Parse(tagInfoArr[1])
                            };
                        }
                    ));
                    _progressBar.Report((double)++progress / ALPHABETS_WITH_123.Length);
                }
                _progressBar.Reset();
                Console.WriteLine("  Complete");
            }

            // add gallery languages and its local names
            hubContext.Clients.All.ReceiveStatus(DbInitStatus.InProgress, "Adding gallery language and types...");
            Console.Write("{0,-" + _totalLeftAlignment + "}", "Adding gallery language and types...");
            string[][] languages = [.. File.ReadAllLines(LANGUAGES_FILE_PATH).Select(pair => pair.Split(delimiter))];
            context.GalleryLanguages.Add(new GalleryLanguage() {
                Id = 1, // manually set Id to 1 to make sure
                IsAll = true,
                EnglishName = "",
                LocalName = ""
            });
            context.GalleryLanguages.AddRange(languages.Select(pair => new GalleryLanguage() {
                IsAll = false, EnglishName = pair[0], LocalName = pair[1]
            }));
            // add gallery types
            string[] types = [.. File.ReadAllLines(TYPES_FILE_PATH)];
            context.GalleryTypes.Add(new GalleryType() {
                Id = 1, // manually set Id to 1 to make sure
                IsAll = true,
                Value = ""
            });
            context.GalleryTypes.AddRange(types.Select(t => new GalleryType() { IsAll = false, Value = t }));
            Console.WriteLine("  Complete");
            Console.Write("{0,-" + _totalLeftAlignment + "}", "Saving changes...");
            context.SaveChanges();
            Console.WriteLine("  Complete");

            // add configurations
            Console.Write("{0,-" + _totalLeftAlignment + "}", "Adding configurations... ");
            hubContext.Clients.All.ReceiveStatus(DbInitStatus.InProgress, "Adding configurations... ");
            context.SearchConfigurations.Add(new() {
                ExampleTagFiltersCreated = false,
                AutoSaveEnabled = true,
                SelectedLanguage = context.GalleryLanguages.First(gl => gl.IsAll),
                SelectedType = context.GalleryTypes.First(gt => gt.IsAll)
            });

            List<GallerySort> sorts = [..Enum.GetValues<GalleryProperty>().Select(s => new GallerySort() {
                Property = s,
                SortDirection = MudBlazor.SortDirection.Ascending,
                IsActive = false
            })];
            // default GallerySort as LastDownloadTime Descending
            GallerySort lastDownloadTimeSort = sorts.First(s => s.Property == GalleryProperty.LastDownloadTime);
            lastDownloadTimeSort.IsActive = true;
            lastDownloadTimeSort.SortDirection = MudBlazor.SortDirection.Descending;
            lastDownloadTimeSort.RankIndex = 0;

            context.BrowseConfigurations.Add(new() {
                SelectedLanguage = context.GalleryLanguages.First(gl => gl.IsAll),
                SelectedType = context.GalleryTypes.First(gt => gt.IsAll),
                ItemsPerPage = 8,
                Sorts = sorts
            });

            context.DownloadConfigurations.Add(new() { ThreadNum = 1 });
            context.ViewConfigurations.Add(new() {
                ViewMode = ViewMode.Default,
                Loop = true,
                ImageLayoutMode = ImageLayoutMode.Automatic,
                ViewDirection = ViewDirection.RTL,
                AutoScrollMode = AutoScrollMode.ByPage,
                PageTurnInterval = 8,
                ScrollSpeed = 1,
                InvertClickNavigation = false,
                InvertKeyboardNavigation = false
            });

            context.AppConfigurations.Add(new() {
                IsFirstLaunch = true,
                AppLanguage = "",
                LastUpdateCheckTime = DateTimeOffset.UtcNow,
            });

            Console.WriteLine("  Complete");
            Console.Write("{0,-" + _totalLeftAlignment + "}", "Saving changes...");
            context.SaveChanges();
            Console.WriteLine("  Complete");
        }
    }
}
