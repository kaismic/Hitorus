using Hitorus.Data.DTOs;
using Hitorus.Data.Events;
using Hitorus.Web.Components.Dialogs;
using Hitorus.Web.Models;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Hitorus.Web.Components {
    public partial class TagFilterEditor : ComponentBase {
        [Inject] IDialogService DialogService { get; set; } = default!;
        [Inject] IStringLocalizer<TagFilterEditor> Localizer { get; set; } = default!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;
        [Inject] private SearchConfigurationService SearchConfigurationService { get; set; } = default!;
        [Parameter, EditorRequired] public ICollection<TagFilterDTO> TagFilters { get; set; } = null!;
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

        private async Task ShowTagFilterSelectorDialog() {
            DialogParameters<SingleTagFilterSelectorDialog> parameters = new() {
                { d => d.ChipModels, [.. TagFilters.Select(tf => new ChipModel<TagFilterDTO>() { Value = tf, Selected = tf.Id == CurrentTagFilter?.Id })] },
            };
            IDialogReference dialogRef = await DialogService.ShowAsync<SingleTagFilterSelectorDialog>(Localizer["Dialog_Title_SelectTagFilter"], parameters);
            DialogResult result = (await dialogRef.Result)!;
            if (!result.Canceled) {
                CurrentTagFilter = (TagFilterDTO)result.Data!;
            }
        }
    }
}