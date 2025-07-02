using Hitorus.Data.Entities;

namespace Hitorus.UnitTests.Api {
    [TestClass]
    public class GalleryTests {
        private MockHitomiContext _mockDbContext;
        [TestInitialize]
        public void Initialize() {
            _mockDbContext?.Dispose();
            _mockDbContext = new();
            _mockDbContext.Database.EnsureDeleted();
            _mockDbContext.Database.EnsureCreated();

            _mockDbContext.SequenceGenerators.Add(new());
            
            // add gallery languages and its local names
            _mockDbContext.GalleryLanguages.AddRange([
                new GalleryLanguage() { EnglishName = "chinese", LocalName = "中文" },
                new GalleryLanguage() { EnglishName = "english", LocalName = "English" },
                new GalleryLanguage() { EnglishName = "japanese", LocalName = "日本語" },
                new GalleryLanguage() { EnglishName = "korean", LocalName = "한국어" },
                new GalleryLanguage() { EnglishName = "russian", LocalName = "Русский" }
            ]);
            // add gallery types
            _mockDbContext.GalleryTypes.AddRange([
                new GalleryType() { Value = "doujinshi" },
                new GalleryType() { Value = "manga" },
                new GalleryType() { Value = "artistcg" },
                new GalleryType() { Value = "gamecg" },
                new GalleryType() { Value = "imageset" },
                new GalleryType() { Value = "anime" }
            ]);
            _mockDbContext.SaveChanges();
        }
    }
}
