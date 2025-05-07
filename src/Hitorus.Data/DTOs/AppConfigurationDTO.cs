namespace Hitorus.Data.DTOs {
    public class AppConfigurationDTO {
        public int Id { get; set; }
        public bool IsFirstLaunch { get; set; }
        public string AppLanguage { get; set; } = "";
        public DateTimeOffset LastUpdateCheckTime { get; set; }
    }
}