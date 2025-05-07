using Hitorus.Data.DTOs;
using Hitorus.Data.Events;
using Hitorus.Web.Models;
using Microsoft.AspNetCore.Components;

namespace Hitorus.Web.Components {
    public partial class TagSearchPanelCollection {
        [Parameter] public string? Style { get; set; }
        [Parameter, EditorRequired] public List<ChipModel<TagDTO>>[] TagSearchPanelChipModels { get; set; } = default!;
        [Parameter] public EventCallback<AdvancedCollectionChangedEventArgs<ChipModel<TagDTO>>> ChipModelsChanged { get; set; }
    }
}
