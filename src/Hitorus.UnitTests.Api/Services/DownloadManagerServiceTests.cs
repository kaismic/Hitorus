using Hitorus.Api.Download;
using Hitorus.Api.Hubs;
using Hitorus.Api.Services;
using Hitorus.Data;
using Hitorus.Data.DbContexts;
using Hitorus.Data.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text;

namespace Hitorus.UnitTests.Api.Services {
    [TestClass]
    public class DownloadManagerServiceTests {
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<ILogger<DownloadManagerService>> _mockLogger;
        private Mock<IEventBus<DownloadEventArgs>> _mockEventBus;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IDbContextFactory<HitomiContext>> _mockDbContextFactory;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private Mock<HitomiContext> _mockDbContext;
        private Mock<DownloadConfiguration> _mockDownloadConfiguration;
        private IDownloadManagerService _downloadManagerService;

        [TestInitialize]
        public void Initialize() {
            // Setup mocks
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockLogger = new Mock<ILogger<DownloadManagerService>>();
            _mockEventBus = new Mock<IEventBus<DownloadEventArgs>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockDbContextFactory = new Mock<IDbContextFactory<HitomiContext>>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockDbContext = new Mock<HitomiContext>();
            _mockDownloadConfiguration = new Mock<DownloadConfiguration>();

            // Setup configuration
            _mockConfiguration.Setup(c => c["HitomiServerDomain"]).Returns("gold-usergeneratedcontent.net");
            _mockConfiguration.Setup(c => c["HitomiClientDomain"]).Returns("hitomi.la");

            // Setup HTTP client
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Setup DbContext
            _mockDownloadConfiguration.SetupGet(c => c.Downloads).Returns(new List<int>());
            _mockDbContext.Setup(c => c.DownloadConfigurations).Returns(CreateDbSetMock(new List<DownloadConfiguration> { _mockDownloadConfiguration.Object }).Object);
            _mockDbContextFactory.Setup(f => f.CreateDbContext()).Returns(_mockDbContext.Object);

            // Create service
            _downloadManagerService = new DownloadManagerService(
                _mockServiceProvider.Object,
                _mockLogger.Object,
                _mockEventBus.Object,
                _mockConfiguration.Object,
                _mockDbContextFactory.Object,
                _mockHttpClientFactory.Object
            );
        }

        [TestMethod]
        public async Task UpdateLiveServerInfo_ShouldUpdateLiveServerInfoProperty() {
            // Arrange
            string ggJsContent = "var c = 0; case 3272: case 2816: 1234567890/'\r\n};";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(ggJsContent, Encoding.UTF8)
                });

            // Get initial hash code
            int initialHashCode = _downloadManagerService.LiveServerInfo.GetHashCode();

            // Act
            await _downloadManagerService.UpdateLiveServerInfo();

            // Assert
            int newHashCode = _downloadManagerService.LiveServerInfo.GetHashCode();
            Assert.AreNotEqual(initialHashCode, newHashCode, "LiveServerInfo should be updated with new values");
            Assert.AreEqual(1234567890, _downloadManagerService.LiveServerInfo.ServerTime);
            Assert.IsTrue(_downloadManagerService.LiveServerInfo.IsContains);
            Assert.AreEqual(2, _downloadManagerService.LiveServerInfo.SubdomainSelectionSet.Count);
            Assert.IsTrue(_downloadManagerService.LiveServerInfo.SubdomainSelectionSet.Contains("3272"));
            Assert.IsTrue(_downloadManagerService.LiveServerInfo.SubdomainSelectionSet.Contains("2816"));
        }

        [TestMethod]
        public void GetOrCreateDownloader_ShouldReturnExistingDownloader_WhenDownloaderExists() {
            // Arrange
            int galleryId = 123;
            var mockDownloader = new Mock<IDownloader>();
            mockDownloader.SetupGet(d => d.GalleryId).Returns(galleryId);

            // Use reflection to access the private _liveDownloaders field
            var liveDownloadersField = typeof(DownloadManagerService).GetField("_liveDownloaders", BindingFlags.NonPublic | BindingFlags.Instance);
            var liveDownloaders = (ConcurrentDictionary<int, IDownloader>)liveDownloadersField.GetValue(_downloadManagerService);
            liveDownloaders.TryAdd(galleryId, mockDownloader.Object);

            // Act
            var result = _downloadManagerService.GetOrCreateDownloader(galleryId, false);

            // Assert
            Assert.AreEqual(mockDownloader.Object, result);
            _mockDbContext.Verify(c => c.SaveChanges(), Times.Never);
        }

        [TestMethod]
        public void GetOrCreateDownloader_ShouldCreateNewDownloader_WhenDownloaderDoesNotExist() {
            // Arrange
            int galleryId = 456;
            var mockServiceScope = new Mock<IServiceScope>();
            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            var mockLogger = new Mock<ILogger<Downloader>>();
            var mockHubContext = new Mock<IHubContext<DownloadHub, IDownloadClient>>();

            mockServiceScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
            _mockServiceProvider.Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(mockServiceScopeFactory.Object);
            _mockServiceProvider.Setup(p => p.GetService(typeof(IConfiguration))).Returns(_mockConfiguration.Object);
            _mockServiceProvider.Setup(p => p.GetService(typeof(ILogger<Downloader>))).Returns(mockLogger.Object);
            _mockServiceProvider.Setup(p => p.GetService(typeof(IHubContext<DownloadHub, IDownloadClient>))).Returns(mockHubContext.Object);
            _mockServiceProvider.Setup(p => p.GetService(typeof(IDbContextFactory<HitomiContext>))).Returns(_mockDbContextFactory.Object);
            _mockServiceProvider.Setup(p => p.GetService(typeof(IEventBus<DownloadEventArgs>))).Returns(_mockEventBus.Object);
            _mockServiceProvider.Setup(p => p.GetService(typeof(IHttpClientFactory))).Returns(_mockHttpClientFactory.Object);
            mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

            // Act
            var result = _downloadManagerService.GetOrCreateDownloader(galleryId, true);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(galleryId, result.GalleryId);
        }

        [TestMethod]
        public void DeleteDownloader_ShouldRemoveDownloaderFromDbAndDispose() {
            // Arrange
            int galleryId = 789;
            var mockDownloader = new Mock<IDownloader>();
            mockDownloader.SetupGet(d => d.GalleryId).Returns(galleryId);

            _mockDownloadConfiguration.Setup(c => c.Downloads.Remove(galleryId)).Returns(true);

            // Act
            _downloadManagerService.DeleteDownloader(mockDownloader.Object, false);

            // Assert
            _mockDownloadConfiguration.Verify(c => c.Downloads.Remove(galleryId), Times.Once);
            _mockDbContext.Verify(c => c.SaveChanges(), Times.Once);
            mockDownloader.Verify(d => d.Dispose(), Times.Once);
        }

        [TestMethod]
        public void OnDownloaderIdChange_ShouldUpdateDownloaderIdInDbAndDictionary() {
            // Arrange
            int oldId = 111;
            int newId = 222;
            var mockDownloader = new Mock<IDownloader>();

            // Use reflection to access the private _liveDownloaders 
            var liveDownloadersField = typeof(DownloadManagerService).GetField("_liveDownloaders", BindingFlags.NonPublic | BindingFlags.Instance);
            var liveDownloaders = (ConcurrentDictionary<int, IDownloader>)liveDownloadersField.GetValue(_downloadManagerService);
            liveDownloaders.TryAdd(oldId, mockDownloader.Object);

            // Act
            _downloadManagerService.OnDownloaderIdChange(oldId, newId);

            // Assert
            Assert.IsFalse(liveDownloaders.ContainsKey(oldId));
            Assert.IsTrue(liveDownloaders.ContainsKey(newId));
        }

        // Helper method to create DbSet mock
        private static Mock<DbSet<T>> CreateDbSetMock<T>(List<T> elements) where T : class {
            var elementsAsQueryable = elements.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();

            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(elementsAsQueryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(elementsAsQueryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(elementsAsQueryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => elementsAsQueryable.GetEnumerator());

            return dbSetMock;
        }
    }
}