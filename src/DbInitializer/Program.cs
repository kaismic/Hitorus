using Hitorus.Data;
using Hitorus.Data.DbContexts;
using Hitorus.Data.Entities;
using MudBlazor;

namespace DbInitializer {
    internal class Program {
        private static readonly string SRC_PATH = Path.Combine("Hitorus", "src");
        private static readonly string ABS_SRC_PATH = Path.Combine(Directory.GetCurrentDirectory().Split(SRC_PATH)[0], SRC_PATH);
        private static readonly string ABS_API_PATH = Path.Combine(ABS_SRC_PATH, "Hitorus.Api");
        private static readonly string ABS_DB_INIT_PATH = Path.Combine(ABS_SRC_PATH, nameof(DbInitializer));
        static void Main() {
            Console.Write("Choose the database to initialize: 1 - main.db, 2 - main-dev.db: ");
            try {
                int arg1 = int.Parse(Console.ReadLine()!);
                if (arg1 < 1 || arg1 > 2) {
                    Console.WriteLine("Invalid input. Please enter 1 or 2.");
                    return;
                }
                string dbName = arg1 == 1 ? "main.db" : "main-dev.db";
                string absoluteDbPath = Path.Combine(ABS_API_PATH, dbName);
                if (File.Exists(absoluteDbPath)) {
                    Console.Write($"{dbName} already exists, are you sure you want to reset the database?");
                    Console.ReadLine();
                }
                Console.WriteLine($"\n--- Deleting and initializing {dbName} database... ---\n");
                using HitomiInitContext dbContext = new(absoluteDbPath);
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();
                Initialize(dbContext);
                Console.WriteLine("\n--- Database initialization complete ---\n");
            } catch (FormatException) {
                Console.WriteLine("Invalid input. Please enter 1 or 2.");
            }
        }

        private static readonly string[] ALPHABETS_WITH_123 =
            ["123", .. Enumerable.Range('a', 26).Select(intValue => Convert.ToChar(intValue).ToString())];

        private static readonly string DB_RES_PATH = Path.Combine(ABS_DB_INIT_PATH, "Resources");
        private static readonly string DELIMITER_FILE_PATH = Path.Combine(DB_RES_PATH, "delimiter.txt");
        private static readonly string LANGUAGES_FILE_PATH = Path.Combine(DB_RES_PATH, "languages.txt");
        private static readonly string TYPES_FILE_PATH = Path.Combine(DB_RES_PATH, "types.txt");

        private static readonly Dictionary<TagCategory, string> CATEGORY_DIR_DICT = new() {
            { TagCategory.Tag, "Tags" },
            { TagCategory.Male, "Males" },
            { TagCategory.Female, "Females" },
            { TagCategory.Artist, "Artists" },
            { TagCategory.Group, "Groups" },
            { TagCategory.Character, "Characters" },
            { TagCategory.Series, "Series" }
        };

        private const int MAX_DESC_TEXT_LENGTH = 40;
        private static readonly ProgressBar _progressBar = new(10);
        private static readonly int _totalLeftAlignment = MAX_DESC_TEXT_LENGTH + _progressBar.TotalLength;

        private static void Initialize(HitomiContext dbContext) {
            string delimiter = File.ReadAllText(DELIMITER_FILE_PATH);
            foreach (TagCategory category in Tag.TAG_CATEGORIES) {
                Console.Write("{0,-" + MAX_DESC_TEXT_LENGTH + "}", $"Adding {category} tags... ");
                int progress = 0;
                string categoryStr = CATEGORY_DIR_DICT[category];
                string dir = Path.Combine(DB_RES_PATH, categoryStr);
                foreach (string alphanumStr in ALPHABETS_WITH_123) {
                    string path = Path.Combine(dir, $"{categoryStr.ToLower()}-{alphanumStr}.txt");
                    string[] tagInfoStrs = File.ReadAllLines(path);
                    dbContext.Tags.AddRange(tagInfoStrs.Select(
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
            Console.Write("{0,-" + _totalLeftAlignment + "}", "Adding gallery language and types...");
            string[][] languages = [.. File.ReadAllLines(LANGUAGES_FILE_PATH).Select(pair => pair.Split(delimiter))];
            dbContext.GalleryLanguages.AddRange(languages.Select(pair => new GalleryLanguage() {
                EnglishName = pair[0],
                LocalName = pair[1]
            }));
            // add gallery types
            string[] types = [.. File.ReadAllLines(TYPES_FILE_PATH)];
            dbContext.GalleryTypes.AddRange(types.Select(value => new GalleryType() { Value = value }));
            Console.WriteLine("  Complete");
            Console.Write("{0,-" + _totalLeftAlignment + "}", "Saving changes...");
            dbContext.SaveChanges();
            Console.WriteLine("  Complete");

            // add configurations
            Console.Write("{0,-" + _totalLeftAlignment + "}", "Adding configurations... ");
            dbContext.SearchConfigurations.Add(new() {
                ExampleTagFiltersCreated = false,
                AutoSaveEnabled = true
            });

            dbContext.BrowseConfigurations.Add(new() {
                ItemsPerPage = 8,
                AutoRefresh = true,
                SelectedSortProperty = GalleryProperty.LastDownloadTime,
                SelectedSortDirection = SortDirection.Descending
            });

            dbContext.DownloadConfigurations.Add(new() {
                DownloadThreadCount = 1,
                MaxConcurrentDownloadCount = 3,
                PreferredFormat = "webp"
            });
            dbContext.ViewConfigurations.Add(new() {
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

            dbContext.AppConfigurations.Add(new() {
                AppThemeColor = "00ffcc",
                AppLaunchCount = 0,
                LastUpdateCheckTime = DateTimeOffset.UtcNow,
                ShowSearchPageWalkthrough = true
            });

            dbContext.SequenceGenerators.Add(new());

            Console.WriteLine("  Complete");
            Console.Write("{0,-" + _totalLeftAlignment + "}", "Saving changes...");
            dbContext.SaveChanges();
            Console.WriteLine("  Complete");
        }
    }
}
