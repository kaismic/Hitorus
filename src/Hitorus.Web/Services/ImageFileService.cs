using Blazored.LocalStorage;

namespace Hitorus.Web.Services {
    public class ImageFileService(IConfiguration hostConfiguration, ISyncLocalStorageService localStorageService) {
        public readonly Uri BASE_IMAGE_URI = Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "ImageServicePath");
    }
}
