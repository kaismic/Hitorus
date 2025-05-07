using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Hitorus.Web.Components {
    public partial class GuidePopover : ComponentBase {
        [Parameter] public string? Class { get; set; }
        [Parameter] public Origin AnchorOrigin { get; set; }
        [Parameter] public Origin TransformOrigin { get; set; }
        [Parameter, EditorRequired] public bool Open { get; set; }
        [Parameter, EditorRequired] public string ContentText { get; set; } = "";
        [Parameter, EditorRequired] public string ButtonText { get; set; } = "";
        [Parameter, EditorRequired] public EventCallback OnClose { get; set; }

        private void OnButtonClick() {
            Open = false;
            OnClose.InvokeAsync();
        }
    }
}
