using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Hitorus.Data.DTOs {
    public class AppConfigurationDTO {
        public int Id { get; set; }
        /// <summary>
        /// <inheritdoc cref="Entities.AppConfiguration.AppThemeColor"/>
        /// </summary>
        [MaxLength(6)]
        [JsonRequired]
        public string AppThemeColor { get; set; } = "";
        [JsonRequired]
        public int AppLaunchCount { get; set; }
        [JsonRequired]
        public bool ShowSurveyPrompt { get; set; } = true;
        public DateTimeOffset LastUpdateCheckTime { get; set; }
        public bool ShowSearchPageWalkthrough { get; set; }
    }
}