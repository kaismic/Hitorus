using Hitorus.Data.DTOs;
using Hitorus.Web.Models;
using Microsoft.AspNetCore.Components;

namespace Hitorus.Web.Components {
    public class PairedTagFilterSelector : TagFilterSelector {
        [Parameter, EditorRequired] public PairedTagFilterSelector Other { get; set; } = default!;

        protected override void OnSelectedChanged(ChipModel<TagFilterDTO> model) {
            base.OnSelectedChanged(model);
            ChipModel<TagFilterDTO>? otherModel = Other.ChipModels.Find(m => m.Value.Id == model.Value.Id);
            if (otherModel != null) {
                otherModel.Disabled = model.Selected;
                Other.StateHasChanged();
            }
        }
    }
}