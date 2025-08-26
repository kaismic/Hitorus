using Hitorus.Data.DTOs;
using Hitorus.Web.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Hitorus.Web.Components {
    public partial class TagFilterSelector : ComponentBase {
        [Parameter] public string? Class { get; set; }
        [Parameter] public string? Style { get; set; }
        [Parameter, EditorRequired] public List<ChipModel<TagFilterDTO>> ChipModels { get; set; } = default!;
        [Parameter] public string? HeaderText { get; set; }
        [Parameter] public Color HeaderColor { get; set; } = Color.Default;
        [Parameter] public IReadOnlyCollection<ChipModel<TagFilterDTO>> SelectedChipModels { get; set; } = [];
        [Parameter] public EventCallback<IReadOnlyCollection<ChipModel<TagFilterDTO>>> SelectedChipModelsChanged { get; set; }
        protected virtual void OnSelectedChanged(ChipModel<TagFilterDTO> model) {}
    }
}