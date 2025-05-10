using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Hitorus.Web.Components.Dialogs {
    public partial class SimpleDialogActions : ComponentBase {
        [Inject] IStringLocalizer<SimpleDialogActions> Localizer { get; set; } = default!;
        [Parameter, EditorRequired] public bool DisableActionButton { get; set; }
        [Parameter, EditorRequired] public EventCallback OnAction { get; set; }
        [Parameter, EditorRequired] public EventCallback OnCancel { get; set; }
    }
}