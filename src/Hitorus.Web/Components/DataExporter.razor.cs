using Hitorus.Data.DTOs;
using Hitorus.Web.Components.Dialogs;
using Hitorus.Web.Models;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;

namespace Hitorus.Web.Components {
    public partial class DataExporter : ComponentBase {
        [Inject] IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] IDialogService DialogService { get; set; } = default!;
        [Inject] IStringLocalizer<DataExporter> Localizer { get; set; } = default!;
        [Inject] TagFilterService TagFilterService { get; set; } = default!;
        [Inject] GalleryService GalleryService { get; set; } = default!;
        [Inject] AppConfigurationService AppConfigurationService { get; set; } = default!;
        [Inject] SearchConfigurationService SearchConfigurationService { get; set; } = default!;
        [Inject] ViewConfigurationService ViewConfigurationService { get; set; } = default!;

        private static readonly Version MinGalleryExportApiVersion = new(1, 1, 0);

        private async Task ExportTagFilters() {
            await SearchConfigurationService.Load();
            IEnumerable<TagFilterDTO> tagFilters = await TagFilterService.GetAllAsync();
            DialogParameters<TagFilterSelectorDialog> parameters = new() {
                { d => d.ChipModels, [.. tagFilters.Select(tf => new ChipModel<TagFilterDTO>() { Value = tf, Selected = true })] }
            };
            IDialogReference dialogRef = await DialogService.ShowAsync<TagFilterSelectorDialog>(Localizer["Dialog_Title_Export"], parameters);
            DialogResult result = (await dialogRef.Result)!;
            if (!result.Canceled) {
                IReadOnlyCollection<ChipModel<TagFilterDTO>> selected = (IReadOnlyCollection<ChipModel<TagFilterDTO>>)result.Data!;
                IEnumerable<int> ids = selected.Select(m => m.Value.Id);
                List<TagFilterBuildDTO> exportingTFs = await TagFilterService.ExportTagFilters(ids);
                await JSRuntime.InvokeVoidAsync("exportData", exportingTFs, "hitorus-tag-filters-" + DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss"), "json");
            }
        }

        private async Task ExportGalleries() {
            IEnumerable<ExportGalleryDTO> galleries = await GalleryService.ExportGalleries();
            await Utilities.ExportData(JSRuntime, galleries, "hitorus-galleries-" + DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss"), "json");
        }

        private async Task ExportAppSettings() {
            await AppConfigurationService.Load(false);
            await Utilities.ExportData(JSRuntime, AppConfigurationService.Config, "hitorus-app-settings" + DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss"), "json");
        }

        private async Task ExportViewPageSettings() {
            await ViewConfigurationService.Load();
            await Utilities.ExportData(JSRuntime, ViewConfigurationService.Config, "hitorus-view-page-settings-" + DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss"), "json");
        }
    }
}
