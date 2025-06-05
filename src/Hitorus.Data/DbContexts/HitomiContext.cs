using Hitorus.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hitorus.Data.DbContexts {
    public class HitomiContext : DbContext {
        public HitomiContext() {}
        public HitomiContext(DbContextOptions<HitomiContext> options) : base(options) { }
        public virtual DbSet<Gallery> Galleries { get; set; }
        public virtual DbSet<GalleryImage> GalleryImages { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<GalleryLanguage> GalleryLanguages { get; set; }
        public virtual DbSet<GalleryType> GalleryTypes { get; set; }
        public virtual DbSet<SearchConfiguration> SearchConfigurations { get; set; }
        public virtual DbSet<BrowseConfiguration> BrowseConfigurations { get; set; }
        public virtual DbSet<DownloadConfiguration> DownloadConfigurations { get; set; }
        public virtual DbSet<ViewConfiguration> ViewConfigurations { get; set; }
        public virtual DbSet<TagFilter> TagFilters { get; set; }
        public virtual DbSet<SearchFilter> SearchFilters { get; set; }
        public virtual DbSet<LabeledTagCollection> LabeledTagCollections { get; set; }
        public virtual DbSet<AppConfiguration> AppConfigurations { get; set; }
    }
}