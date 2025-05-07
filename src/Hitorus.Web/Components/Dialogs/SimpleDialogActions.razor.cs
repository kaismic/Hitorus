using Microsoft.AspNetCore.Components;

namespace Hitorus.Web.Components.Dialogs {
    public partial class SimpleDialogActions : ComponentBase {
        [Parameter, EditorRequired] public string ActionText { get; set; } = null!;
        [Parameter, EditorRequired] public bool DisableActionButton { get; set; }
        [Parameter, EditorRequired] public EventCallback OnAction { get; set; }
        [Parameter, EditorRequired] public EventCallback OnCancel { get; set; }
    }
}