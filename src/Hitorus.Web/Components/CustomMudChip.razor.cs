using Hitorus.Web.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Hitorus.Web.Components {
    public partial class CustomMudChip<T> : ComponentBase {
        private string? _modelId;
        [Parameter, EditorRequired] public ChipModel<T> Model { get; set; } = default!;
        [Parameter, EditorRequired] public Func<ChipModel<T>, string> ToStringFunc { get; set; } = default!;
        [Parameter] public EventCallback<MudChip<ChipModel<T>>> OnClose { get; set; }
        [Parameter] public EventCallback<ChipModel<T>> OnSelectedChanged { get; set; }

        protected override void OnParametersSet() {
            if (_modelId != Model.Id) {
                _modelId = Model.Id;
                Model.SelectedChanged = OnSelectedChanged;
            }
        }
    }
}