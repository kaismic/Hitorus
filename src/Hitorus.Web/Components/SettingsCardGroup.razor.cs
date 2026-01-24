using Microsoft.AspNetCore.Components;

namespace Hitorus.Web.Components {
    public partial class SettingsCardGroup : ComponentBase {
        [Parameter] public RenderFragment? ChildContent { get; set; }
    }
}
