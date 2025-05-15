using Hitorus.Data.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Hitorus.Data.Entities {
    public class AppConfiguration {
        public int Id { get; set; }
        public bool IsFirstLaunch { get; set; }
        public string AppLanguage { get; set; } = ""; // empty string means automatic
        public DateTimeOffset LastUpdateCheckTime { get; set; }
        /// <summary>
        /// The value is in rgb hex format without the number sign, e.g. "FFCC00" where r = FF, g = CC, b = 00
        /// </summary>
        [MaxLength(6)] public string AppThemeColor { get; set; } = "";

        public AppConfigurationDTO ToDTO() => new() {
            Id = Id,
            IsFirstLaunch = IsFirstLaunch,
            AppLanguage = AppLanguage,
            LastUpdateCheckTime = LastUpdateCheckTime,
            AppThemeColor = AppThemeColor
        };
    }
}