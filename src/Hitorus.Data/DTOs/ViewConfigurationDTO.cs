using System.Text.Json.Serialization;

namespace Hitorus.Data.DTOs;
public class ViewConfigurationDTO {
    public int Id { get; set; }
    [JsonRequired]
    public ViewMode ViewMode { get; set; }
    [JsonRequired]
    public bool Loop { get; set; }
    [JsonRequired]
    public ImageLayoutMode ImageLayoutMode { get; set; }
    [JsonRequired]
    public ViewDirection ViewDirection { get; set; }
    [JsonRequired]
    public AutoScrollMode AutoScrollMode { get; set; }
    [JsonRequired]
    public int PageTurnInterval { get; set; }
    [JsonRequired]
    public int ScrollSpeed { get; set; }
    [JsonRequired]
    public bool InvertClickNavigation { get; set; }
    [JsonRequired]
    public bool InvertKeyboardNavigation { get; set; }
}