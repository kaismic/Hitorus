using Hitorus.Data.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Hitorus.Web.Components {
    public partial class GallerySortItemView : ComponentBase {
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;
        [Inject] IStringLocalizer<GallerySortItemView> Localizer { get; set; } = default!;
        [Parameter, EditorRequired] public GallerySortDTO GallerySort { get; set; } = default!;
        [Parameter, EditorRequired] public EventCallback<GallerySortDTO> OnDeleteClick { get; set; } = default!;
    }
}
