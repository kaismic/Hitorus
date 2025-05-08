using System.Globalization;

namespace Hitorus.Web {
    public class Utilities {
        /// <summary>
        /// somehow you gotta set both CurrentCulture and CurrentUICulture to make localization work
        /// </summary>
        /// <param name="value"></param>
        public static void SetAppLanguage(string value) {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo(value);
            CultureInfo.DefaultThreadCurrentUICulture= CultureInfo.GetCultureInfo(value);
        }
    }
}
