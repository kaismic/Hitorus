using Hitorus.Data.DTOs;
using Hitorus.Web.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Hitorus.Web.Components.Dialogs {
    public partial class SingleTagFilterSelectorDialog : ComponentBase {
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;
        [Parameter, EditorRequired] public List<ChipModel<TagFilterDTO>> ChipModels { get; set; } = default!;

        private void OnChipModelClicked(ChipModel<TagFilterDTO> model) {
            MudDialog.Close(DialogResult.Ok(model.Value));
        }
    }
}
