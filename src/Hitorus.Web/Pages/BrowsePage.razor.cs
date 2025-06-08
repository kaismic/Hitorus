using Blazored.LocalStorage;
using Hitorus.Data;
using Hitorus.Data.DTOs;
using Hitorus.Data.Entities;
using Hitorus.Data.Events;
using Hitorus.Web.Models;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;

namespace Hitorus.Web.Pages {
    public partial class BrowsePage : ComponentBase, IDisposable {
        [Inject] BrowseConfigurationService BrowseConfigurationService { get; set; } = default!;
        [Inject] GalleryService GalleryService { get; set; } = default!;
        [Inject] IJSRuntime JsRuntime {get;set;} = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;
        [Inject] IStringLocalizer<BrowsePage> Localizer { get; set; } = default!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;
        [Inject] ILocalStorageService LocalStorageService { get; set; } = default!;

        private int _thumbnailImageCount;
        private readonly List<ChipModel<TagDTO>>[] _tagSearchPanelChipModels = [.. Tag.TAG_CATEGORIES.Select(t => new List<ChipModel<TagDTO>>())];

        private bool _isLoading = false;
        private bool _isEditing = false;

        private async Task OnSelectedLanguageChanged(GalleryLanguageDTO value) {
            BrowseConfigurationService.Config.SelectedLanguage = value;
            await BrowseConfigurationService.UpdateLanguageAsync(value.Id);
            if (BrowseConfigurationService.Config.AutoRefresh) {
                await LoadGalleries();
            }
        }
        
        private async Task OnSelectedTypeChanged(GalleryTypeDTO value) {
            BrowseConfigurationService.Config.SelectedType = value;
            await BrowseConfigurationService.UpdateTypeAsync(value.Id);
            if (BrowseConfigurationService.Config.AutoRefresh) {
                await LoadGalleries();
            }
        }
        
        private async Task OnTitleSearchKeywordChanged(string value) {
            BrowseConfigurationService.Config.TitleSearchKeyword = value;
            await BrowseConfigurationService.UpdateTitleSearchKeywordAsync(value);
            if (BrowseConfigurationService.Config.AutoRefresh) {
                await LoadGalleries();
            }
        }

        private async Task OnItemsPerPageChanged(int value) {
            BrowseConfigurationService.Config.ItemsPerPage = value;
            await BrowseConfigurationService.UpdateItemsPerPageAsync(value);
            if (BrowseConfigurationService.Config.AutoRefresh) {
                await LoadGalleries();
            }
        }

        private async Task OnThumbnailImageCountChanged(int value) {
            _thumbnailImageCount = value;
            await LocalStorageService.SetItemAsync(LocalStorageKeys.THUMBNAIL_IMAGE_COUNT, value);
            await LoadGalleries();
        }

        private async Task OnSelectedSortPropertyChanged(GalleryProperty value) {
            BrowseConfigurationService.Config.SelectedSortProperty = value;
            await BrowseConfigurationService.UpdateSelectedSortPropertyAsync(value);
            if (BrowseConfigurationService.Config.AutoRefresh) {
                await LoadGalleries();
            }
        }

        private async Task OnSelectedSortDirectionChanged(SortDirection value) {
            BrowseConfigurationService.Config.SelectedSortDirection = value;
            await BrowseConfigurationService.UpdateSelectedSortDirectionAsync(value);
            if (BrowseConfigurationService.Config.AutoRefresh) {
                await LoadGalleries();
            }
        }

        private async Task OnAutoRefreshChanged(bool value) {
            BrowseConfigurationService.Config.AutoRefresh = value;
            await BrowseConfigurationService.UpdateAutoRefreshAsync(value);
        }

        private bool _isInitialized = false;
        private bool _isRendered = false;

        protected override async Task OnInitializedAsync() {
            await BrowseConfigurationService.Load();
            _thumbnailImageCount = await LocalStorageService.GetItemAsync<int>(LocalStorageKeys.THUMBNAIL_IMAGE_COUNT);
            _isInitialized = true;
            _ = OnInitRenderComplete();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                await JsRuntime.InvokeVoidAsync("setHeightToSourceHeight", "tag-search-panel-collection", "class", "ltk-search-view", "class");
                _isRendered = true;
                _ = OnInitRenderComplete();
            }
        }

        private async Task OnInitRenderComplete() {
            if (_isInitialized && _isRendered) {
                BrowseConfigurationService.LoadGalleries = () => _ = LoadGalleries();
                BrowseConfigurationService.BrowsePageLoaded = true;
                for (int i = 0; i < Tag.TAG_CATEGORIES.Length; i++) {
                    TagCategory category = Tag.TAG_CATEGORIES[i];
                    IEnumerable<TagDTO> tags = BrowseConfigurationService.Config.Tags.Where(t => t.Category == category);
                    foreach (TagDTO tag in tags) {
                        _tagSearchPanelChipModels[i].Add(new ChipModel<TagDTO> { Value = tag });
                    }
                }
                if (BrowseConfigurationService.BrowsePageFirstLoad || BrowseConfigurationService.BrowsePageRefreshQueued) {
                    BrowseConfigurationService.BrowsePageRefreshQueued = false;
                    BrowseConfigurationService.BrowsePageFirstLoad = false;
                    await LoadGalleries();
                }
                StateHasChanged();
            }
        }

        private void OnChipModelsChanged(AdvancedCollectionChangedEventArgs<ChipModel<TagDTO>> e) {
            _ = Task.Run(async () => {
                switch (e.Action) {
                    case AdvancedCollectionChangedAction.AddSingle: {
                        BrowseConfigurationService.Config.Tags.Add(e.NewItem!.Value);
                        await BrowseConfigurationService.AddTagsAsync([e.NewItem!.Value.Id]);
                        break;
                    }
                    case AdvancedCollectionChangedAction.RemoveMultiple: {
                        HashSet<int> removingIds = [.. e.OldItems!.Select(m => m.Value.Id)];
                        BrowseConfigurationService.Config.Tags.RemoveAll(t => removingIds.Contains(t.Id));
                        await BrowseConfigurationService.RemoveTagsAsync(removingIds);
                        break;
                    }
                    case AdvancedCollectionChangedAction.RemoveSingle: {
                        int id = e.OldItem!.Value.Id;
                        BrowseConfigurationService.Config.Tags.RemoveAll(t => t.Id == id);
                        await BrowseConfigurationService.RemoveTagsAsync([id]);
                        break;
                    }
                    default:
                        throw new NotImplementedException();
                }
                 if (BrowseConfigurationService.Config.AutoRefresh) {
                     _ = LoadGalleries();
                 }
            });
        }

        private async Task OnPageNumChanged(int value) {
            BrowseConfigurationService.PageNum = value;
            await LoadGalleries();
        }

        public async Task LoadGalleries() {
            _isLoading = true;
            StateHasChanged();
            BrowseQueryResult result = await GalleryService.GetBrowseQueryResult(BrowseConfigurationService.PageNum - 1, BrowseConfigurationService.Config.Id);
            BrowseConfigurationService.TotalPages = result.TotalGalleryCount / BrowseConfigurationService.Config.ItemsPerPage + 
                Math.Min(result.TotalGalleryCount % BrowseConfigurationService.Config.ItemsPerPage, 1);
            HashSet<int> ids = [.. result.GalleryIds];
            List<BrowseGalleryDTO> galleries = new(ids.Count);
            foreach (int id in ids) {
                if (BrowseConfigurationService.GalleryCache.TryGet(id, out BrowseGalleryDTO? gallery)) {
                    galleries.Add(gallery);
                    ids.Remove(id);
                }
            }
            if (ids.Count > 0) {
                List<BrowseGalleryDTO> temp = await GalleryService.GetBrowseGalleryDTOs(ids);
                foreach (BrowseGalleryDTO gallery in temp) {
                    BrowseConfigurationService.GalleryCache.AddOrUpdate(gallery.Id, gallery);
                    galleries.Add(gallery);
                }
            }
            // need to set _selections before setting _galleries
            BrowseConfigurationService.Selections = new bool[galleries.Count];
            BrowseConfigurationService.Galleries = galleries;
            _isLoading = false;
            StateHasChanged();
        }

        private async Task DeleteGalleries() {
            List<int> ids = [];
            for (int i = 0; i < BrowseConfigurationService.Selections.Length; i++) {
                if (BrowseConfigurationService.Selections[i]) {
                    ids.Add(BrowseConfigurationService.Galleries[i].Id);
                }
            }
            bool success = await GalleryService.DeleteGalleries(ids);
            if (success) {
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_MultiGalleryDeleteSuccess"], ids.Count),
                    Severity.Success,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
                await LoadGalleries();
            } else {
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_MultiGalleryDeleteFailure"]),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
            }
        }

        private void ExitEditMode() {
            _isEditing = false;
            for (int i = 0; i < BrowseConfigurationService.Selections.Length; i++) {
                BrowseConfigurationService.Selections[i] = false;
            }
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
            BrowseConfigurationService.BrowsePageLoaded = false;
        }
    }
}