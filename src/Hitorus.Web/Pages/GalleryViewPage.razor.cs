using BlazorPro.BlazorSize;
using Hitorus.Data;
using Hitorus.Data.DTOs;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text;

namespace Hitorus.Web.Pages {
    public partial class GalleryViewPage : IDisposable {
        [Inject] IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] IResizeListener ResizeListener { get; set; } = default!;
        [Inject] IConfiguration HostConfiguration { get; set; } = default!;
        [Inject] IStringLocalizer<GalleryViewPage> Localizer { get; set; } = default!;
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;
        [Inject] GalleryService GalleryService { get; set; } = default!;
        [Inject] AppConfigurationService AppConfigurationService { get; set; } = default!;
        [Inject] ViewConfigurationService ViewConfigurationService { get; set; } = default!;
        [Parameter] public int GalleryId { get; set; }

        private const string DEFAULT_TOOLBAR_HEIGHT = "80px";

        private const string JAVASCRIPT_FILE = $"./Pages/{nameof(GalleryViewPage)}.razor.js";
        private IJSObjectReference? _jsModule;
        private MudThemeProvider _mudThemeProvider = null!;
        private bool _isDarkMode;

        private ViewGalleryDTO? _gallery;
        private ViewConfigurationDTO _viewConfiguration = new();
        private string _baseImageUrl = "";
        /// <summary>
        /// 0-based image index ranges. Start is inclusive, End is exclusive.
        /// </summary>
        private Range[] _imageIndexRanges = [];
        /// <summary>
        /// 0-based page index
        /// </summary>
        private int PageIndex => PageNumber - 1;
        private int PageNumber { get; set; } = 1;
        private int _pageOffset = 0;
        private BrowserWindowSize _browserWindowSize = new();
        private bool _isAutoScrolling = false;
        private CancellationTokenSource? _autoPageTurnCts;
        private FitMode _fitMode = FitMode.Auto;
        private int _imagesPerPage = 2;
        private DotNetObjectReference<GalleryViewPage>? _dotNetObjectRef;
        private bool _preventDefaultKeyDown = false;
        private bool _toolbarOpen = false;

        protected override void OnInitialized() {
            _baseImageUrl = HostConfiguration["ApiUrl"] + HostConfiguration["ImageFilePath"] + "?galleryId=" + GalleryId;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                _isDarkMode = await _mudThemeProvider.GetSystemPreference();
                if (!ViewConfigurationService.IsLoaded) {
                    await ViewConfigurationService.Load();
                }
                _viewConfiguration = ViewConfigurationService.Config;
                _gallery ??= await GalleryService.GetViewGalleryDTO(GalleryId);
                _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
                _dotNetObjectRef = DotNetObjectReference.Create(this);
                await _jsModule.InvokeVoidAsync("setDotNetObject", _dotNetObjectRef);
                await _jsModule.InvokeVoidAsync("init", _viewConfiguration);
                CaculateImageIndexGroups();
                ResizeListener.OnResized += OnResize;
            }
        }

        private bool _pageNumberChangedByJs = false;
        private async Task OnPageNumberChanged(int value) {
            PageNumber = value;
            if (_viewConfiguration.ViewMode == ViewMode.Scroll && !_pageNumberChangedByJs) {
                _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
                await _jsModule.InvokeVoidAsync("scrollToIndex", PageIndex);
            }
            StateHasChanged();
        }

        /// <summary>
        /// <paramref name="pageNumber"/> must be 1-based.
        /// </summary>
        /// <param name="pageNumber"></param>
        [JSInvokable]
        public async Task SetPageNumberFromJs(int pageNumber) {
            _pageNumberChangedByJs = true;
            await OnPageNumberChanged(pageNumber);
            _pageNumberChangedByJs = false;
        }

        private bool CanDecrement() => _viewConfiguration.Loop || PageIndex > 0;
        private bool CanIncrement() => _viewConfiguration.Loop || PageIndex < _imageIndexRanges.Length - 1;
        private async Task Decrement() {
            if (PageNumber == 1) {
                await OnPageNumberChanged(_imageIndexRanges.Length);
            } else {
                await OnPageNumberChanged((PageNumber - 1 + _imageIndexRanges.Length) % _imageIndexRanges.Length);
            }
            StateHasChanged();
        }
        private async Task Increment() {
            if (PageNumber == _imageIndexRanges.Length) {
                await OnPageNumberChanged(1);
            } else if (PageNumber == _imageIndexRanges.Length - 1) {
                await OnPageNumberChanged(_imageIndexRanges.Length);
            } else {
                await OnPageNumberChanged((PageNumber + 1) % _imageIndexRanges.Length);
            }
            StateHasChanged();
        }

        private string GetImageStyle() {
            StringBuilder sb = new("object-fit: scale-down; ");
            sb.Append("max-width: ");
            if (_fitMode == FitMode.Vertical) {
                sb.Append("auto");
            } else {
                int imageCount;
                switch (_viewConfiguration.ImageLayoutMode) {
                    case ImageLayoutMode.Automatic:
                        Range currRange = _imageIndexRanges[PageIndex];
                        imageCount = currRange.End.Value - currRange.Start.Value;
                        break;
                    case ImageLayoutMode.Fixed:
                        imageCount = _imagesPerPage;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                sb.Append($"{100.0 / imageCount}%");
            }
            sb.Append("; max-height: ");
            if (_fitMode == FitMode.Horizontal) {
                sb.Append("auto");
            } else {
                sb.Append($"calc(100dvh - {DEFAULT_TOOLBAR_HEIGHT})");
            }
            sb.Append(';');
            return sb.ToString();
        }

        private void OnPageOffsetChanged(int offset) {
            _pageOffset = offset;
            CaculateImageIndexGroups();
        }

        private async Task OnViewModeChanged(ViewMode value) {
            _viewConfiguration.ViewMode = value;
            _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
            await _jsModule.InvokeVoidAsync("setViewMode", value);
            if (value == ViewMode.Scroll) {
                await Task.Delay(50);
                await _jsModule.InvokeVoidAsync("scrollToIndex", PageIndex);
            }
        }

        private async Task OnAutoScrollModeChanged(AutoScrollMode value) {
            _viewConfiguration.AutoScrollMode = value;
            _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
            await _jsModule.InvokeVoidAsync("setAutoScrollMode", value);
        }
        
        private async Task OnLoopChanged(bool value) {
            _viewConfiguration.Loop = value;
            _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
            await _jsModule.InvokeVoidAsync("setLoop", value);
        }

        private async Task OnScrollSpeedChanged(int value) {
            _viewConfiguration.ScrollSpeed = value;
            _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
            await _jsModule.InvokeVoidAsync("setScrollSpeed", value);
        }
        
        private async Task OnPageTurnIntervalChanged(int value) {
            _viewConfiguration.PageTurnInterval = value;
            _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
            await _jsModule.InvokeVoidAsync("setPageTurnInterval", value);
        }

        private void OnImageLayoutModeChanged(ImageLayoutMode mode) {
            _viewConfiguration.ImageLayoutMode = mode;
            CaculateImageIndexGroups();
        }


        private void ToggleAutoScroll(bool value) {
            _isAutoScrolling = value;
            switch (_viewConfiguration.ViewMode) {
                case ViewMode.Default:
                    if (value) {
                        _autoPageTurnCts = new();
                        _ = StartAutoPageTurn();
                    } else {
                        _autoPageTurnCts?.Cancel();
                        _autoPageTurnCts?.Dispose();
                    }
                    break;
                case ViewMode.Scroll:
                    _ = Task.Run(async () => {
                        _jsModule ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", JAVASCRIPT_FILE);
                        if (value) {
                            await _jsModule.InvokeVoidAsync("startAutoScroll");
                        } else {
                            await _jsModule.InvokeVoidAsync("stopAutoScroll");
                        }
                    });
                    break;
            }
        }

        /// <summary>
        /// This method must be called only when ViewMode is Default.
        /// </summary>
        /// <returns></returns>
        private async Task StartAutoPageTurn() {
            while (_isAutoScrolling) {
                await Task.Delay(_viewConfiguration.PageTurnInterval * 1000, _autoPageTurnCts!.Token);
                if (CanIncrement()) {
                    await Increment();
                } else {
                    ToggleAutoScroll(false);
                }
                StateHasChanged();
            }
        }

        private void CaculateImageIndexGroups() {
            if (_gallery == null) {
                return;
            }
            // consider offset, view direction, images per page, image layout mode
            List<Range> indexRanges = [];
            // add offset number of images at first page index range
            if (_pageOffset > 0) {
                indexRanges.Add(new(0, _pageOffset));
            }
            switch (_viewConfiguration.ImageLayoutMode) {
                case ImageLayoutMode.Automatic:
                    double viewportAspectRatio = (double)_browserWindowSize.Width / _browserWindowSize.Height;
                    double remainingAspectRatio = viewportAspectRatio - (double)_gallery.Images.ElementAt(0).Width / _gallery.Images.ElementAt(0).Height;
                    int currStart = _pageOffset;
                    int count = 1;
                    for (int i = _pageOffset + 1; i < _gallery.Images.Count; i++) {
                        double currImgAspectRatio = (double)_gallery.Images.ElementAt(i).Width / _gallery.Images.ElementAt(i).Height;
                        if (currImgAspectRatio > remainingAspectRatio || count == _imagesPerPage) {
                            remainingAspectRatio = viewportAspectRatio - currImgAspectRatio;
                            indexRanges.Add(new(currStart, i));
                            currStart = i;
                            count = 1;
                        } else {
                            count++;
                            remainingAspectRatio -= currImgAspectRatio;
                        }
                    }
                    // add last range
                    indexRanges.Add(new(currStart, _gallery.Images.Count));
                    break;
                case ImageLayoutMode.Fixed:
                    int quotient = (_gallery.Images.Count - _pageOffset) / _imagesPerPage;
                    int remainder = (_gallery.Images.Count - _pageOffset) % _imagesPerPage;
                    IEnumerable<Range> midRanges = Enumerable.Range(0, quotient)
                        .Select(i => new Range(
                            i * _imagesPerPage + _pageOffset,
                            (i + 1) * _imagesPerPage + _pageOffset)
                        );
                    indexRanges.AddRange(midRanges);
                    if (remainder > 0) {
                        indexRanges.Add(new(_gallery.Images.Count - remainder, _gallery.Images.Count));
                    }
                    break;
            }
            if (PageIndex > indexRanges.Count) {
                PageNumber = 1;
            }
            _imageIndexRanges = [.. indexRanges];
            StateHasChanged();
        }

        private async Task OnKeyDown(KeyboardEventArgs e) {
            _preventDefaultKeyDown = true;
            switch (e.Code) {
                case "ArrowLeft" or "ArrowRight":
                    if (e.Code == "ArrowLeft" ^ ViewConfigurationService.Config.InvertKeyboardNavigation) {
                        if (CanDecrement()) await Decrement();
                    } else {
                        if (CanIncrement()) await Increment();
                    }
                    break;
                case "ArrowDown" or "ArrowUp":
                    if (_viewConfiguration.ViewMode == ViewMode.Default) {
                        return;
                    }
                    if (e.Code == "ArrowUp") {
                        if (CanDecrement()) await Decrement();
                    } else {
                        if (CanIncrement()) await Increment();
                    }
                    break;
                case "Space":
                    ToggleAutoScroll(!_isAutoScrolling);
                    break;
                default:
                    _preventDefaultKeyDown = false;
                    break;
            }
        }

        private async Task OnWheel(WheelEventArgs e) {
            if (e.DeltaY < 0 && CanDecrement()) {
                await Decrement();
            } else if (e.DeltaY > 0 && CanIncrement()) {
                await Increment();
            }
        }

        private async Task OnPageClick(MouseEventArgs e) {
            if (e.Button == 0) {
                bool isXOverHalf = e.ClientX > (_browserWindowSize.Width / 2);
                if (CanIncrement() && isXOverHalf ^ ViewConfigurationService.Config.InvertClickNavigation) {
                    await Increment();
                } else if (CanDecrement() && !(isXOverHalf ^ ViewConfigurationService.Config.InvertClickNavigation)) {
                    await Decrement();
                }
            }
        }

        private void OnResize(object? sender, BrowserWindowSize size) {
            ToggleAutoScroll(false);
            _browserWindowSize = size;
            CaculateImageIndexGroups();
        }

        [JSInvokable]
        public void OnAutoScrollStop() {
            _isAutoScrolling = false;
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
            if (_autoPageTurnCts != null) {
                _autoPageTurnCts.Cancel();
                _autoPageTurnCts.Dispose();
            }
            _dotNetObjectRef?.Dispose();
            ResizeListener.OnResized -= OnResize;
        }
    }
}
