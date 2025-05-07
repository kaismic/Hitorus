using Microsoft.AspNetCore.Components;

namespace Hitorus.Web.Models {
    public class ChipModel<TValue> {
        public string Id { get; } = $"chip-{Guid.NewGuid()}";
        public required TValue Value { get; init; }
        public bool Disabled { get; set; } = false;
        private bool _selected = false;
        public bool Selected {
            get => _selected;
            set {
                _selected = value;
                SelectedChanged.InvokeAsync(this);
            }
        }
        public EventCallback<ChipModel<TValue>> SelectedChanged;
    }
}
