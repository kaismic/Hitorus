using Hitorus.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DbInitializer {
    internal class HitomiInitContext(string dbPath) : HitomiContext {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
