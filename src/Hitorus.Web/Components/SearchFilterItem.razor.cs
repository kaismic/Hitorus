using Hitorus.Data.DTOs;
using Microsoft.AspNetCore.Components;

namespace Hitorus.Web.Components {
    public partial class SearchFilterItem : ComponentBase {
        [Parameter, EditorRequired] public SearchFilterDTO Model { get; set; } = default!;
    }
}
