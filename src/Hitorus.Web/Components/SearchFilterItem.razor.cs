using Hitorus.Data.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Hitorus.Web.Components {
    public partial class SearchFilterItem : ComponentBase {
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;
        [Parameter, EditorRequired] public SearchFilterDTO Model { get; set; } = default!;
    }
}
