using Microsoft.AspNetCore.Components;

namespace Hitorus.Web.Components {
    public partial class SettingsCard : ComponentBase {
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }
        [Parameter] public string Icon { get; set; } = "";
        [Parameter] public string Header { get; set; } = "";
        [Parameter] public string Description { get; set; } = "";
        [Parameter] public RenderFragment? ChildContent { get; set; }
    }
}