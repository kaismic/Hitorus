using System.ComponentModel.DataAnnotations;

namespace Hitorus.Data.DTOs {
    public class AppConfigurationDTO {
        public int Id { get; set; }
        /// <summary>
        /// <inheritdoc cref="Entities.AppConfiguration.AppLanguage"/>
        /// </summary>
        public string AppLanguage { get; set; } = "";
        /// <summary>
        /// <inheritdoc cref="Entities.AppConfiguration.AppThemeColor"/>
        /// </summary>
        [MaxLength(6)] public string AppThemeColor { get; set; } = "";
        public int AppLaunchCount { get; set; }
        public bool ShowSurveyPrompt { get; set; } = true;
        public DateTimeOffset LastUpdateCheckTime { get; set; }
        public bool ShowSearchPageWalkthrough { get; set; }
    }
}