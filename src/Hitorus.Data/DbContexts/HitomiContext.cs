using Hitorus.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hitorus.Data.DbContexts {
    public class HitomiContext : DbContext {
        public static readonly string MAIN_DATABASE_PATH = "main.db";
        public HitomiContext() {}

        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<GalleryLanguage> GalleryLanguages { get; set; }
        public DbSet<GalleryType> GalleryTypes { get; set; }
        public DbSet<GallerySort> GallerySorts { get; set; }
        public DbSet<SearchConfiguration> SearchConfigurations { get; set; }
        public DbSet<BrowseConfiguration> BrowseConfigurations { get; set; }
        public DbSet<DownloadConfiguration> DownloadConfigurations { get; set; }
        public DbSet<ViewConfiguration> ViewConfigurations { get; set; }
        public DbSet<TagFilter> TagFilters { get; set; }
        public DbSet<SearchFilter> SearchFilters { get; set; }
        public DbSet<LabeledTagCollection> LabeledTagCollections { get; set; }
        public DbSet<AppConfiguration> AppConfigurations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder
                .UseSqlite($"Data Source={MAIN_DATABASE_PATH}")
                .EnableSensitiveDataLogging();
        }
    }
}