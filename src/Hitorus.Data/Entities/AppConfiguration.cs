using Hitorus.Data.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Hitorus.Data.Entities {
    public class AppConfiguration {
        public int Id { get; set; }
        /// <summary>
        /// Empty string means automatic.
        /// </summary>
        public string AppLanguage { get; set; } = "";
        /// <summary>
        /// The value is in rgb hex format without the number sign, e.g. "FFCC00" where r = FF, g = CC, b = 00
        /// </summary>
        [MaxLength(6)] public string AppThemeColor { get; set; } = "";
        public int AppLaunchCount { get; set; } = 0;
        public bool ShowSurveyPrompt { get; set; } = true;
        public DateTimeOffset LastUpdateCheckTime { get; set; }
        public bool ShowSearchPageWalkthrough { get; set; } = true;

        public AppConfigurationDTO ToDTO() => new() {
            Id = Id,
            AppLanguage = AppLanguage,
            AppThemeColor = AppThemeColor,
            AppLaunchCount = AppLaunchCount,
            ShowSurveyPrompt = ShowSurveyPrompt,
            LastUpdateCheckTime = LastUpdateCheckTime,
            ShowSearchPageWalkthrough = ShowSearchPageWalkthrough
        };
    }
}