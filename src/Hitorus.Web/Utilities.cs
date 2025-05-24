using Blazored.LocalStorage;
using System.Globalization;

namespace Hitorus.Web {
    public class Utilities {
        public static uint ArgbToRgba(uint value) {
            return (value - 0xFF000000) * 0x100 + 0xFF;
        }

        public static uint RgbToArgb(string value) {
            return 0xFF000000 + uint.Parse(value, NumberStyles.HexNumber);
        }

        public static int GetApiPort(IConfiguration hostConfiguration, ISyncLocalStorageService localStorageService) {
            return localStorageService.GetItem<int?>(LocalStorageKeys.API_PORT) ?? hostConfiguration.GetValue<int>("DefaultApiPort");
        }

        public static Uri GetServiceBaseUri(
            IConfiguration hostConfiguration,
            ISyncLocalStorageService localStorageService,
            string pathKey
        ) {
            UriBuilder builder = new() {
                Scheme = "http",
                Host = hostConfiguration["BaseApiHost"],
                Port = GetApiPort(hostConfiguration, localStorageService),
                Path = hostConfiguration[pathKey]
            };
            return builder.Uri;
        }
    }
}
