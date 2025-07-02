using Hitorus.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Hitorus.UnitTests.Api {
    internal class MockHitomiContext : HitomiContext {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlite($"Data Source=test.db");
        }
    }
}
