using Hitorus.Data.DTOs;

namespace Hitorus.Data.Entities {
    public class AppConfiguration {
        public int Id { get; set; }
        public bool IsFirstLaunch { get; set; }
        public string AppLanguage { get; set; } = ""; // empty string means automatic
        public DateTimeOffset LastUpdateCheckTime { get; set; }

        public AppConfigurationDTO ToDTO() => new() {
            Id = Id,
            IsFirstLaunch = IsFirstLaunch,
            AppLanguage = AppLanguage,
            LastUpdateCheckTime = LastUpdateCheckTime
        };
    }
}