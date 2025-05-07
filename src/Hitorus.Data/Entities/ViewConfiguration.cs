using Hitorus.Data.DTOs;

namespace Hitorus.Data.Entities;
public class ViewConfiguration {
    public int Id { get; set; }
    public ViewMode ViewMode { get; set; }
    /// <summary>
    /// When ImageLayoutMode == Automatic, this represents the maximum number of images to display per page.
    /// When ImageLayoutMode == Fixed, this represents the fixed number of images to display per page.
    /// </summary>
    public bool Loop { get; set; }
    public ImageLayoutMode ImageLayoutMode { get; set; }
    public ViewDirection ViewDirection { get; set; }
    public AutoScrollMode AutoScrollMode { get; set; }
    public int PageTurnInterval { get; set; } // in seconds
    public int ScrollSpeed { get; set; } // in pixels per x milliseconds (see startAutoScroll function in GalleryViewPage.razor.js for exact value)
    public bool InvertClickNavigation { get; set; }
    public bool InvertKeyboardNavigation { get; set; }

    public ViewConfigurationDTO ToDTO() => new() {
        Id = Id,
        ViewMode = ViewMode,
        Loop = Loop,
        ImageLayoutMode = ImageLayoutMode,
        ViewDirection = ViewDirection,
        PageTurnInterval = PageTurnInterval,
        AutoScrollMode = AutoScrollMode,
        ScrollSpeed = ScrollSpeed,
        InvertClickNavigation = InvertClickNavigation,
        InvertKeyboardNavigation = InvertKeyboardNavigation
    };
}