using Hitorus.Data.DTOs;
using Microsoft.AspNetCore.Components;

namespace Hitorus.Web.Components {
    public partial class GallerySortItemView : ComponentBase {
        [Parameter, EditorRequired] public GallerySortDTO GallerySort { get; set; } = default!;
        [Parameter, EditorRequired] public EventCallback<GallerySortDTO> OnDeleteClick { get; set; } = default!;
    }
}
