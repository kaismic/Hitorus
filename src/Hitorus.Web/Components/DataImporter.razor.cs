using Hitorus.Data.DTOs;
using Hitorus.Web.Components.Dialogs;
using Hitorus.Web.Models;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using MudBlazor;
using System.Text.Json;

namespace Hitorus.Web.Components {
    public partial class DataImporter : ComponentBase {
        [CascadingParameter] private Action LayoutStateHasChanged { get; set; } = default!;
        [Inject] IStringLocalizer<DataImporter> Localizer { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;
        [Inject] IDialogService DialogService { get; set; } = default!;
        [Inject] TagFilterService TagFilterService { get; set; } = default!;
        [Inject] GalleryService GalleryService { get; set; } = default!;
        [Inject] AppConfigurationService AppConfigurationService { get; set; } = default!;
        [Inject] SearchConfigurationService SearchConfigurationService { get; set; } = default!;
        [Inject] ViewConfigurationService ViewConfigurationService { get; set; } = default!;

        private const long MB_IN_BYTES = 1_000_000;
        private const long MAX_FILE_SIZE = MB_IN_BYTES * 10;
        private static readonly JsonSerializerOptions DEFAULT_JSON_SERIALIZER_OPTIONS = new() {
            PropertyNameCaseInsensitive = true,
        };

        private async Task ImportTagFilters(InputFileChangeEventArgs args) {
            if (args.File.Size > MAX_FILE_SIZE) {
                Snackbar.Add(
                    string.Format(
                        Localizer["Snackbar_ImportFileTooLarge"],
                        ((double)args.File.Size / MB_IN_BYTES) + "mb",
                        (MAX_FILE_SIZE / MB_IN_BYTES) + "mb"
                    ),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                return;
            }
            using Stream fileStream = args.File.OpenReadStream(MAX_FILE_SIZE);
            List<TagFilterBuildDTO>? candidates;
            try {
                candidates = await JsonSerializer.DeserializeAsync<List<TagFilterBuildDTO>>(
                    fileStream,
                    DEFAULT_JSON_SERIALIZER_OPTIONS
                );
            } catch (JsonException e) {
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_ImportInvalidFormat"], e.Message),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                return;
            }
            if (candidates == null) {
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_ImportUnknownError"]),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                return;
            }
            if (candidates.Count == 0) {
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_ImportEmptyFile"]),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                return;
            }
            Dictionary<string, TagFilterBuildDTO> candidateDict = candidates.ToDictionary(dto => dto.Name, dto => dto);
            DialogParameters<TagFilterSelectorDialog> parameters = new() {
                { d => d.ChipModels, [.. candidates.Select(tf => tf.ToDTO()).Select(tf => new ChipModel<TagFilterDTO>() { Value = tf, Selected = true })] }
            };
            IDialogReference dialogRef = await DialogService.ShowAsync<TagFilterSelectorDialog>(Localizer["Dialog_Title_Import"], parameters);
            DialogResult result = (await dialogRef.Result)!;
            if (!result.Canceled) {
                IReadOnlyCollection<ChipModel<TagFilterDTO>> selected = (IReadOnlyCollection<ChipModel<TagFilterDTO>>)result.Data!;
                IEnumerable<string> selectedNames = selected.Select(m => m.Value.Name);
                IEnumerable<TagFilterBuildDTO> selectedValues = selectedNames.Select(name => candidateDict[name]);
                List<TagFilterDTO> imported = await TagFilterService.ImportTagFilters(selectedValues);
                foreach (TagFilterDTO tf in imported) {
                    SearchConfigurationService.Config.TagFilters.Add(tf);
                }
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_ImportSuccess"], imported.Count),
                    Severity.Success,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
            }
        }

        private async Task ImportGalleries(InputFileChangeEventArgs args) {
            if (args.File.Size > MAX_FILE_SIZE) {
                Snackbar.Add(
                    string.Format(
                        Localizer["Snackbar_ImportFileTooLarge"],
                        ((double)args.File.Size / MB_IN_BYTES) + "mb",
                        (MAX_FILE_SIZE / MB_IN_BYTES) + "mb"
                    ),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                return;
            }
            using Stream fileStream = args.File.OpenReadStream(MAX_FILE_SIZE);
            List<ExportGalleryDTO>? candidates;
            try {
                candidates = await JsonSerializer.DeserializeAsync<List<ExportGalleryDTO>>(
                    fileStream,
                    DEFAULT_JSON_SERIALIZER_OPTIONS
                );
            } catch (JsonException e) {
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_ImportInvalidFormat"], e.Message),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                return;
            }
            if (candidates == null) {
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_ImportUnknownError"]),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                return;
            }
            if (candidates.Count == 0) {
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_ImportEmptyFile"]),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                return;
            }
            int importedCount = await GalleryService.ImportGalleries(candidates);
            Snackbar.Add(
                $"Imported {importedCount} galleries.",
                Severity.Success,
                UiConstants.DEFAULT_SNACKBAR_OPTIONS
            );
        }


        private async Task ImportAppSettings(InputFileChangeEventArgs args) {
            if (args.File.Size > MAX_FILE_SIZE) {
                Snackbar.Add(
                    string.Format(
                        Localizer["Snackbar_ImportFileTooLarge"],
                        ((double)args.File.Size / MB_IN_BYTES) + "mb",
                        (MAX_FILE_SIZE / MB_IN_BYTES) + "mb"
                    ),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                return;
            }
            using Stream fileStream = args.File.OpenReadStream(MAX_FILE_SIZE);
            AppConfigurationDTO? dto;
            try {
                dto = await JsonSerializer.DeserializeAsync<AppConfigurationDTO>(
                    fileStream,
                    DEFAULT_JSON_SERIALIZER_OPTIONS
                );
            } catch (JsonException e) {
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_ImportInvalidFormat"], e.Message),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                return;
            }
            if (dto == null) {
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_ImportUnknownError"]),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                return;
            }
            bool success = await AppConfigurationService.Import(dto);
            Snackbar.Add(
                "App settings import success.",
                Severity.Success,
                UiConstants.DEFAULT_SNACKBAR_OPTIONS
            );
            LayoutStateHasChanged();
        }

        private async Task ImportViewPageSettings(InputFileChangeEventArgs args) {
            if (args.File.Size > MAX_FILE_SIZE) {
                Snackbar.Add(
                    string.Format(
                        Localizer["Snackbar_ImportFileTooLarge"],
                        ((double)args.File.Size / MB_IN_BYTES) + "mb",
                        (MAX_FILE_SIZE / MB_IN_BYTES) + "mb"
                    ),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                return;
            }
            using Stream fileStream = args.File.OpenReadStream(MAX_FILE_SIZE);
            ViewConfigurationDTO? dto;
            try {
                dto = await JsonSerializer.DeserializeAsync<ViewConfigurationDTO>(
                    fileStream,
                    DEFAULT_JSON_SERIALIZER_OPTIONS
                );
            } catch (JsonException e) {
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_ImportInvalidFormat"], e.Message),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                return;
            }
            if (dto == null) {
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_ImportUnknownError"]),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                return;
            }
            bool success = await ViewConfigurationService.Import(dto);
            Snackbar.Add(
                "View settings import success.",
                Severity.Success,
                UiConstants.DEFAULT_SNACKBAR_OPTIONS
            );
            LayoutStateHasChanged();
        }
    }
}
