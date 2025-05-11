using Hitorus.Data.Builders;
using Hitorus.Data.DTOs;
using Hitorus.Data.Entities;
using Hitorus.Data.Events;
using Hitorus.Web.Components;
using Hitorus.Web.Components.Dialogs;
using Hitorus.Web.Models;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Hitorus.Web.Pages {
    public partial class SearchPage {
        [Inject] TagFilterService TagFilterService { get; set; } = default!;
        [Inject] SearchFilterService SearchFilterService { get; set; } = default!;
        [Inject] SearchConfigurationService SearchConfigurationService { get; set; } = default!;
        [Inject] AppConfigurationService AppConfigurationService { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;
        [Inject] IDialogService DialogService { get; set; } = default!;
        [Inject] IJSRuntime JsRuntime { get; set; } = default!;
        [Inject] IConfiguration HostConfiguration { get; set; } = default!;
        [Inject] IStringLocalizer<SearchPage> Localizer { get; set; } = default!;

        private ObservableCollection<TagFilterDTO> _tagFilters = [];
        public ObservableCollection<TagFilterDTO> TagFilters {
            get => _tagFilters;
            set {
                if (_tagFilters == value) {
                    return;
                }
                _tagFilters = value;
                SearchConfigurationService.Config.TagFilters = value;
                _includeTagFilterChipModels = [.. value.Select(tf => new ChipModel<TagFilterDTO>() { Value = tf })];
                _excludeTagFilterChipModels = [.. value.Select(tf => new ChipModel<TagFilterDTO>() { Value = tf })];
                value.CollectionChanged += TagFiltersChanged;
                StateHasChanged();
            }
        }

        private void TagFiltersChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    List<TagFilterDTO> newTfs = [.. e.NewItems!.Cast<TagFilterDTO>()];
                    _includeTagFilterChipModels.InsertRange(e.NewStartingIndex, [.. newTfs.Select(tf => new ChipModel<TagFilterDTO>() { Value = tf })]);
                    _excludeTagFilterChipModels.InsertRange(e.NewStartingIndex, [.. newTfs.Select(tf => new ChipModel<TagFilterDTO>() { Value = tf })]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // assumes ChipModels has the same order in regards to TagFilterDTO.Id
                    _includeTagFilterChipModels.RemoveRange(e.OldStartingIndex, e.OldItems!.Count);
                    _excludeTagFilterChipModels.RemoveRange(e.OldStartingIndex, e.OldItems!.Count);
                    break;
                default:
                    break;
            }
        }

        private PairedTagFilterSelector _includePairedTagFilterSelector = null!;
        private PairedTagFilterSelector _excludePairedTagFilterSelector = null!;
        private List<ChipModel<TagFilterDTO>> _includeTagFilterChipModels = [];
        private List<ChipModel<TagFilterDTO>> _excludeTagFilterChipModels = [];

        private TagFilterEditor _tagFilterEditor = null!;
        private readonly List<ChipModel<TagDTO>>[] _tagSearchPanelChipModels = [.. Tag.TAG_CATEGORIES.Select(t => new List<ChipModel<TagDTO>>())];

        private async Task OnSelectedLanguageChanged(GalleryLanguageDTO value) {
            SearchConfigurationService.Config.SelectedLanguage = value;
            await SearchConfigurationService.UpdateLanguageAsync(value.Id);
        }

        private async Task OnSelectedTypeChanged(GalleryTypeDTO value) {
            SearchConfigurationService.Config.SelectedType = value;
            await SearchConfigurationService.UpdateTypeAsync(value.Id);
        }

        private async Task OnTitleSearchKeywordChanged(string value) {
            SearchConfigurationService.Config.TitleSearchKeyword = value;
            await SearchConfigurationService.UpdateTitleSearchKeywordAsync(value);
        }

        private async Task OnAutoSaveEnabledChanged(bool value) {
            SearchConfigurationService.Config.AutoSaveEnabled = value;
            await SearchConfigurationService.UpdateAutoSaveEnabledAsync(value);
        }

        private ObservableCollection<SearchFilterDTO> _searchFilters = [];
        public ObservableCollection<SearchFilterDTO> SearchFilters {
            get => _searchFilters;
            set {
                if (_searchFilters == value) {
                    return;
                }
                _searchFilters = value;
                SearchConfigurationService.Config.SearchFilters = value;
                value.CollectionChanged += SearchFiltersChanged;
            }
        }

        private void SearchFiltersChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Reset:
                    _ = SearchFilterService.ClearAsync();
                    break;
                // Add and Remove are handled in CreateSearchFilter and DeleteSearchFilter
                default:
                    break;
            }
        }

        private bool _isInitialized = false;
        private bool _isRendered = false;
        private bool _isLoaded = false;

        protected override async Task OnInitializedAsync() {
            _isInitialized = false;
            _isRendered = false;
            await SearchConfigurationService.Load();
            await AppConfigurationService.Load();
            TagFilters = [.. SearchConfigurationService.Config.TagFilters];
            SearchFilters = [.. SearchConfigurationService.Config.SearchFilters];
            if (AppConfigurationService.Config.IsFirstLaunch) {
                _showWalkthrough = true;
            } else if (AppConfigurationService.Config.LastUpdateCheckTime.AddDays(HostConfiguration.GetValue<int>("UpdateCheckInterval")) < DateTimeOffset.UtcNow) {
                Version? recentVersion = await AppConfigurationService.GetRecentVersion();
                if (recentVersion != null && recentVersion > AppConfigurationService.CURRENT_APP_VERSION) {
                    Snackbar.Add(
                        $"A new app version is available: {recentVersion.Major}.{recentVersion.Minor}.{recentVersion.Build}",
                        Severity.Success,
                        options => {
                            options.ShowCloseIcon = true;
                            options.CloseAfterNavigation = false;
                            options.ShowTransitionDuration = 0;
                            options.HideTransitionDuration = 500;
                            options.VisibleStateDuration = 10000;
                        }
                    );
                }
                await AppConfigurationService.UpdateAutoUpdateCheckTime(DateTimeOffset.UtcNow);
            }
            _isInitialized = true;
            OnInitRenderComplete();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                await JsRuntime.InvokeVoidAsync("setFillHeightResizeObserver", "tag-search-panel-collection", "class", "search-page-left-container", "id");
                _isRendered = true;
                OnInitRenderComplete();
            }
        }

        private void OnInitRenderComplete() {
            if (_isInitialized && _isRendered) {
                _tagFilterEditor.CurrentTagFilter = TagFilters.FirstOrDefault(tf => tf.Id == SearchConfigurationService.Config.SelectedTagFilterId);
                foreach (ChipModel<TagFilterDTO> chipModel in _includeTagFilterChipModels) {
                    if (SearchConfigurationService.Config.SelectedIncludeTagFilterIds.Contains(chipModel.Value.Id)) {
                        chipModel.Selected = true;
                    }
                }
                foreach (ChipModel<TagFilterDTO> chipModel in _excludeTagFilterChipModels) {
                    if (SearchConfigurationService.Config.SelectedExcludeTagFilterIds.Contains(chipModel.Value.Id)) {
                        chipModel.Selected = true;
                    }
                }
                _isLoaded = true;
            }
        }

        private void OnSelectedTagFilterCollectionChanged(IReadOnlyCollection<ChipModel<TagFilterDTO>> collection, bool isInclude) {
            if (!_isLoaded) {
                return;
            }
            IEnumerable<int> ids = collection.Select(m => m.Value.Id);
            if (isInclude) {
                SearchConfigurationService.Config.SelectedIncludeTagFilterIds = ids;
            } else {
                SearchConfigurationService.Config.SelectedExcludeTagFilterIds = ids;
            }
            _ = SearchConfigurationService.UpdateSelectedTagFilterCollectionAsync(isInclude, ids);
        }

        private async Task SelectedTagFilterChanged(ValueChangedEventArgs<TagFilterDTO> args) {
            if (SearchConfigurationService.Config.AutoSaveEnabled) {
                await SaveTagFilter(args.OldValue);
            }
            if (args.NewValue == null) {
                ClearAllTags();
            } else {
                await LoadTags();
            }
        }

        private void ClearAllTags() {
            for (int i = 0; i < _tagSearchPanelChipModels.Length; i++) {
                _tagSearchPanelChipModels[i].Clear();
            }
        }

        private async Task LoadTags() {
            if (_tagFilterEditor.CurrentTagFilter != null) {
                IEnumerable<TagDTO> tags = await TagFilterService.GetTagsAsync(_tagFilterEditor.CurrentTagFilter.Id);
                if (tags != null) {
                    for (int i = 0; i < Tag.TAG_CATEGORIES.Length; i++) {
                        _tagSearchPanelChipModels[i] = [..
                            tags.Where(t => t.Category == Tag.TAG_CATEGORIES[i])
                                .OrderBy(t => t.Value.Length)
                                .Select(t => new ChipModel<TagDTO>() { Value = t })
                        ];
                    }
                }
                StateHasChanged();
            }
        }

        private async Task CreateTagFilter() {
            // to prevent enter key acting on the last clicked button
            await JsRuntime.InvokeVoidAsync("document.body.focus");
            DialogParameters<TextFieldDialog> parameters = new() {
                { d => d.TextFieldLabel, Localizer["TagFilterName"] }
            };
            IDialogReference dialogRef = await DialogService.ShowAsync<TextFieldDialog>(Localizer["Dialog_Title_CreateTagFilter"], parameters);
            ((TextFieldDialog)dialogRef.Dialog!).AddValidators(IsDuplicate);
            DialogResult result = (await dialogRef.Result)!;
            if (!result.Canceled) {
                string name = result.Data!.ToString()!;
                TagFilterBuildDTO buildDto = new() {
                    SearchConfigurationId = SearchConfigurationService.Config.Id,
                    Name = name,
                    Tags = _tagSearchPanelChipModels.SelectMany(l => l).Select(m => m.Value)
                };
                TagFilterDTO tagFilter = buildDto.ToDTO();
                tagFilter.Id = await TagFilterService.CreateAsync(buildDto);
                TagFilters.Add(tagFilter);
                _tagFilterEditor.CurrentTagFilter = tagFilter;
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_Msg_CreateTagFilter_Success"], name),
                    Severity.Success,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
            }
        }

        private async Task RenameTagFilter() {
            // to prevent enter key acting on the last clicked button
            await JsRuntime.InvokeVoidAsync("document.body.focus");
            string oldName = _tagFilterEditor.CurrentTagFilter!.Name;
            DialogParameters<TextFieldDialog> parameters = new() {
                { d => d.TextFieldLabel, Localizer["TagFilterName"] },
                { d => d.Text, oldName }
            };
            IDialogReference dialogRef = await DialogService.ShowAsync<TextFieldDialog>(Localizer["Dialog_Title_RenameTagFilter"], parameters);
            ((TextFieldDialog)dialogRef.Dialog!).AddValidators(IsDuplicate);
            DialogResult result = (await dialogRef.Result)!;
            if (!result.Canceled) {
                string name = result.Data!.ToString()!;
                bool success = await TagFilterService.UpdateNameAsync(_tagFilterEditor.CurrentTagFilter!.Id, name);
                if (success) {
                    _tagFilterEditor.CurrentTagFilter.Name = name;
                    Snackbar.Add(
                        string.Format(Localizer["Snackbar_Msg_RenameTagFilter_Success"], oldName, name),
                        Severity.Success,
                        UiConstants.DEFAULT_SNACKBAR_OPTIONS
                    );
                } else {
                    Snackbar.Add(
                        string.Format(Localizer["Snackbar_Msg_RenameTagFilter_Failure"], oldName, name),
                        Severity.Error,
                        UiConstants.DEFAULT_SNACKBAR_OPTIONS
                    );
                }
            }
        }

        private string? IsDuplicate(string name) {
            if (TagFilters.Any(tf => tf.Name == name)) {
                return string.Format(Localizer["Validation_Msg_Duplicate"], name);
            }
            return null;
        }

        private async Task SaveTagFilter(TagFilterDTO? tagFilter) {
            if (tagFilter != null && TagFilters.Contains(tagFilter) /* tag filter could have been deleted */) {
                bool success = await TagFilterService.UpdateTagsAsync(
                    tagFilter.Id,
                    _tagSearchPanelChipModels.SelectMany(l => l).Select(m => m.Value.Id)
                );
                if (success) {
                    Snackbar.Add(
                        string.Format(Localizer["Snackbar_Msg_SaveTagFilter_Success"], tagFilter.Name),
                        Severity.Success,
                        UiConstants.DEFAULT_SNACKBAR_OPTIONS
                    );
                } else {
                    Snackbar.Add(
                        string.Format(Localizer["Snackbar_Msg_SaveTagFilter_Failure"], tagFilter.Name),
                        Severity.Error,
                        UiConstants.DEFAULT_SNACKBAR_OPTIONS
                    );
                }
            }
        }

        private async Task DeleteTagFilters() {
            DialogParameters<TagFilterSelectorDialog> parameters = new() {
                { d => d.ChipModels, [.. TagFilters.Select(tf => new ChipModel<TagFilterDTO>() { Value = tf })] }
            };
            IDialogReference dialogRef = await DialogService.ShowAsync<TagFilterSelectorDialog>(Localizer["Dialog_Title_DeleteTagFilter"], parameters);
            DialogResult result = (await dialogRef.Result)!;
            if (!result.Canceled) {
                IReadOnlyCollection<ChipModel<TagFilterDTO>> selected = (IReadOnlyCollection<ChipModel<TagFilterDTO>>)result.Data!;
                IEnumerable<int> ids = selected.Select(m => m.Value.Id);
                bool success = await TagFilterService.DeleteAsync(ids);
                if (success) {
                    TagFilters = [.. TagFilters.ExceptBy(ids, tf => tf.Id)];
                    Snackbar.Add(
                        string.Format(Localizer["Snackbar_Msg_DeleteTagFilter_Success"], selected.Count),
                        Severity.Success,
                        UiConstants.DEFAULT_SNACKBAR_OPTIONS
                    );
                    if (_tagFilterEditor.CurrentTagFilter != null && selected.Any(m => m.Value.Id == _tagFilterEditor.CurrentTagFilter.Id)) {
                        _tagFilterEditor.CurrentTagFilter = null;
                    }
                } else {
                    Snackbar.Add(
                        string.Format(Localizer["Snackbar_Msg_DeleteTagFilter_Failure"]),
                        Severity.Error,
                        UiConstants.DEFAULT_SNACKBAR_OPTIONS
                    );
                }
            }
        }

        private async Task CreateSearchFilter() {
            HashSet<int> includeIds = [.. SearchConfigurationService.Config.SelectedIncludeTagFilterIds];
            HashSet<int> excludeIds = [.. SearchConfigurationService.Config.SelectedExcludeTagFilterIds];
            bool currentTagFilterInclude = false;
            bool currentTagFilterExclude = false;
            if (_tagFilterEditor.CurrentTagFilter != null) {
                if (includeIds.Contains(_tagFilterEditor.CurrentTagFilter.Id)) {
                    currentTagFilterInclude = true;
                    includeIds.Remove(_tagFilterEditor.CurrentTagFilter.Id);
                } else if (excludeIds.Contains(_tagFilterEditor.CurrentTagFilter.Id)) {
                    currentTagFilterExclude = true;
                    excludeIds.Remove(_tagFilterEditor.CurrentTagFilter.Id);
                }
            }
            Task<IEnumerable<TagDTO>>? includeTagsTask = null;
            Task<IEnumerable<TagDTO>>? excludeTagsTask = null;
            if (includeIds.Count > 0) {
                includeTagsTask = TagFilterService.GetTagsUnionAsync(includeIds);
            }
            if (excludeIds.Count > 0) {
                excludeTagsTask = TagFilterService.GetTagsUnionAsync(excludeIds);
            }
            await Task.WhenAll(includeTagsTask ?? Task.CompletedTask, excludeTagsTask ?? Task.CompletedTask);
            IEnumerable<TagDTO> includeTagDTOs = includeTagsTask?.Result ?? [];
            IEnumerable<TagDTO> excludeTagDTOs = excludeTagsTask?.Result ?? [];
            IEnumerable<TagDTO> currentTagDTOs = _tagSearchPanelChipModels.SelectMany(l => l).Select(m => m.Value);
            if (currentTagFilterInclude) {
                includeTagDTOs = includeTagDTOs.Union(currentTagDTOs);
            } else if (currentTagFilterExclude) {
                excludeTagDTOs = excludeTagDTOs.Union(currentTagDTOs);
            }
            Dictionary<int, TagDTO> includeDict = includeTagDTOs.ToDictionary(dto => dto.Id);
            HashSet<int> duplicateIds = [.. includeDict.Keys];
            duplicateIds.IntersectWith(excludeTagDTOs.Select(t => t.Id));
            if (duplicateIds.Count > 0) {
                string contentText = string.Join(
                    ", ",
                    duplicateIds.Select(id => includeDict[id])
                                .Select(tag => tag.Category.ToString() + ':' + tag.Value)
                );
                DialogParameters<NotificationDialog> parameters = new() {
                    { d => d.HeaderText, Localizer["Dialog_Header_ConflictingTags"] },
                    { d => d.ContentText, contentText },
                };
                await DialogService.ShowAsync<NotificationDialog>(Localizer["Dialog_Title_ConflictingTags"], parameters);
                return;
            }

            SearchFilterDTOBuilder builder = new() {
                Language = SearchConfigurationService.Config.SelectedLanguage,
                Type = SearchConfigurationService.Config.SelectedType,
                TitleSearchKeyword = SearchConfigurationService.Config.TitleSearchKeyword,
                IncludeTags = includeTagDTOs,
                ExcludeTags = excludeTagDTOs
            };
            SearchFilterDTO dto = builder.Build();
            SearchFilters.Add(dto);
            await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", dto.SearchLink);
            int id = await SearchFilterService.CreateAsync(dto);
            dto.Id = id;
        }

        private async Task DeleteSearchFilter(SearchFilterDTO dto) {
            bool success = await SearchFilterService.DeleteAsync(dto.Id);
            if (success) {
                SearchFilters.Remove(dto);
            } else {
                Snackbar.Add(
                    string.Format(Localizer["Snackbar_Msg_DeleteSearchLink_Failure"]),
                    Severity.Error,
                    UiConstants.DEFAULT_SNACKBAR_OPTIONS
                );
            }
        }

        private bool _showWalkthrough = false;
        private const int WALKTHROUGH_STEPS = 4;
        private int _walkthroughStep = 0;

        private async Task ShowNextGuide() {
            _walkthroughStep++;
            if (_walkthroughStep >= WALKTHROUGH_STEPS) {
                _showWalkthrough = false;
                AppConfigurationService.Config.IsFirstLaunch = false;
                await AppConfigurationService.UpdateIsFirstLaunch(false);
                await AppConfigurationService.UpdateAutoUpdateCheckTime(DateTimeOffset.UtcNow);
            }
        }

        private string GetWalkthroughHighlightStyle(int step) {
            return _showWalkthrough && _walkthroughStep == step ? "z-index: calc(var(--mud-zindex-popover) + 1); pointer-events: none;" : "";
        }
    }
}
