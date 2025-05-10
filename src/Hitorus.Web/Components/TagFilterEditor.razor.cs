using Hitorus.Data.DTOs;
using Hitorus.Data.Events;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Hitorus.Web.Components {
    public partial class TagFilterEditor : ComponentBase {
        [Inject] IStringLocalizer<TagFilterEditor> Localizer { get; set; } = default!;
        [Inject] private SearchConfigurationService SearchConfigurationService { get; set; } = default!;
        [Parameter, EditorRequired] public IEnumerable<TagFilterDTO> TagFilters { get; set; } = null!;
        [Parameter, EditorRequired] public EventCallback OnCreateButtonClicked { get; set; }
        [Parameter, EditorRequired] public EventCallback OnRenameButtonClicked { get; set; }
        [Parameter, EditorRequired] public EventCallback OnSaveButtonClicked { get; set; }
        [Parameter, EditorRequired] public EventCallback OnDeleteButtonClicked { get; set; }
        [Parameter] public EventCallback<ValueChangedEventArgs<TagFilterDTO>> SelectedTagFilterChanged { get; set; }
        [Parameter, EditorRequired] public string SelectPlaceholder { get; set; } = default!;

        private bool _firstTagFilter = true;
        private TagFilterDTO? _currentTagFilter;
        public TagFilterDTO? CurrentTagFilter {
            get => _currentTagFilter;
            set {
                if (_currentTagFilter == value) {
                    return;
                }
                TagFilterDTO? oldValue = _currentTagFilter;
                _currentTagFilter = value;
                SearchConfigurationService.Config.SelectedTagFilterId = value?.Id ?? 0;
                // this check prevents unnecessary selected tag filter update request
                if (!_firstTagFilter || SearchConfigurationService.IsInitTagFilterNull) {
                    _ = SearchConfigurationService.UpdateSelectedTagFilterAsync(SearchConfigurationService.Config.SelectedTagFilterId);
                } else {
                    _firstTagFilter = false;
                }
                SelectedTagFilterChanged.InvokeAsync(new(oldValue, value));
            }
        }
    }
}