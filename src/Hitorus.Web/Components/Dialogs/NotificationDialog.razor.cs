using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Hitorus.Web.Components.Dialogs {
    public partial class NotificationDialog : ComponentBase {
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
        [Inject] IStringLocalizer<NotificationDialog> Localizer { get; set; } = default!;
        [Parameter, EditorRequired] public string HeaderText { get; set; } = null!;
        [Parameter, EditorRequired] public string ContentText { get; set; } = null!;
        private void Close() => MudDialog.Close(DialogResult.Cancel());
    }
}
