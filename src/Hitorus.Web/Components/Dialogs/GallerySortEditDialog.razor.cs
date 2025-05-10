using Hitorus.Data.DTOs;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Hitorus.Web.Components.Dialogs {
    public partial class GallerySortEditDialog : ComponentBase {
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
        [Inject] IStringLocalizer<GallerySortEditDialog> Localizer { get; set; } = default!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;
        [Inject] private BrowseConfigurationService BrowseConfigurationService { get; set; } = default!;

        private MudDropContainer<GallerySortDTO> _dropContainer = default!;
        private MudDropZone<GallerySortDTO> _dropZone = default!;
        private ICollection<GallerySortDTO> _sorts = [];

        protected override void OnInitialized() {
            _sorts = [.. BrowseConfigurationService.Config.Sorts.Select(s => new GallerySortDTO() {
                Property = s.Property,
                SortDirection = s.SortDirection,
                IsActive = s.IsActive,
                RankIndex = s.RankIndex
            })];
        }

        private void AddSort(GallerySortDTO sort) {
            sort.IsActive = true;
            _dropContainer.Refresh();
            StateHasChanged();
        }

        private void RemoveSort(GallerySortDTO sort) {
            sort.IsActive = false;
            _dropContainer.Refresh();
            StateHasChanged();
        }

        public void ExecuteAction() {
            GallerySortDTO[] activeSorts = _dropZone.GetItems();
            for (int i = 0; i < activeSorts.Length; i++) {
                activeSorts[i].RankIndex = i;
            }
            MudDialog.Close(DialogResult.Ok(_sorts));
        }

        private void Close() => MudDialog.Close(DialogResult.Cancel());
    }
}
