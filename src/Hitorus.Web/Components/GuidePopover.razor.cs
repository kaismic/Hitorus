using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Hitorus.Web.Components {
    public partial class GuidePopover : ComponentBase {
        [Parameter] public string? Class { get; set; }
        [Parameter] public Origin AnchorOrigin { get; set; }
        [Parameter] public Origin TransformOrigin { get; set; }
        [Parameter, EditorRequired] public bool Open { get; set; }
        [Parameter, EditorRequired] public string ContentText { get; set; } = "";
        [Parameter, EditorRequired] public string NextButtonText { get; set; } = "";
        [Parameter] public string SkipButtonText { get; set; } = "";
        [Parameter] public bool ShowSkipButton { get; set; } = false;
        [Parameter, EditorRequired] public EventCallback OnClose { get; set; }
        [Parameter] public EventCallback OnSkip { get; set; }

        private void OnNextButtonClick() {
            Open = false;
            OnClose.InvokeAsync();
        }

        private void OnSkipButtonClick() {
            Open = false;
            OnSkip.InvokeAsync();
        }
    }
}
