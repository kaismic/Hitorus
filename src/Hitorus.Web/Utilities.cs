using System.Globalization;

namespace Hitorus.Web {
    public class Utilities {
        /// <summary>
        /// somehow you gotta set both CurrentCulture and CurrentUICulture to make localization work
        /// </summary>
        /// <param name="value"></param>
        public static void SetAppLanguage(string value) {
            try {
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo(value);
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo(value);
            } catch (CultureNotFoundException) { }
        }

        public static uint ArgbToRgba(uint value) {
            return (value - 0xFF000000) * 0x100 + 0xFF;
        }

        public static uint RgbToArgb(string value) {
            return 0xFF000000 + uint.Parse(value, NumberStyles.HexNumber);
        }
    }
}
