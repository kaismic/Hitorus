using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Hitorus.Web.Components.Dialogs {
    public partial class SurveyPromptDialog : ComponentBase {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;
        [Inject] IStringLocalizer<SurveyPromptDialog> Localizer { get; set; } = default!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;
        private bool _doNotShowAgain = false;
        private void Close() => MudDialog.Close(DialogResult.Ok(_doNotShowAgain));
    }
}