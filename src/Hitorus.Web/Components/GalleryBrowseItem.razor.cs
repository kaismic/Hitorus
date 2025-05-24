using BlazorPro.BlazorSize;
using Hitorus.Data.DTOs;
using Hitorus.Data.Entities;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;

namespace Hitorus.Web.Components {
    public partial class GalleryBrowseItem : ComponentBase, IDisposable {
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private IResizeListener ResizeListener { get; set; } = default!;
        [Inject] private IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;
        [Inject] private IStringLocalizer<GalleryBrowseItem> Localizer { get; set; } = default!;
        [Inject] private LanguageTypeService LanguageTypeService { get; set; } = default!;
        [Inject] ImageFileService ImageFileService { get; set; } = default!;
        [Parameter, EditorRequired] public BrowseGalleryDTO Gallery { get; set; } = default!;
        [Parameter, EditorRequired] public bool IsEditing { get; set; }
        [Parameter, EditorRequired] public bool IsSelected { get; set; }
        [Parameter] public EventCallback<bool> IsSelectedChanged { get; set; }

        private string _imageContainerId = "";
        private const int THUMBNAIL_IMAGE_HEIGHT = 120; // px
        private double _maxRecordedAspectRatio;
        private double[] _cumulativeImageAspectRatios = [];
        private int _maxImageCount = 1;
        private string _baseImageUrl = "";
        private readonly List<KeyValuePair<TagCategory, List<TagDTO>>> _tagCollections = [];

        protected override void OnInitialized() {
            _imageContainerId = "thumbnail-image-container-" + Gallery.Id;
            UriBuilder builder = new(ImageFileService.BASE_IMAGE_URI) {
                Query = "?galleryId=" + Gallery.Id
            };
            _baseImageUrl = builder.ToString();
            List<GalleryImageDTO> images = [.. Gallery.Images];
            _cumulativeImageAspectRatios = new double[images.Count];
            _cumulativeImageAspectRatios[0] = (double)images[0].Width / images[0].Height;
            _maxRecordedAspectRatio = _cumulativeImageAspectRatios[0];
            for (int i = 1; i < Gallery.Images.Count; i++) {
                _cumulativeImageAspectRatios[i] = _cumulativeImageAspectRatios[i - 1] + (double)images[i].Width / images[i].Height;
            }
        }

        protected override void OnAfterRender(bool firstRender) {
            if (firstRender) {
                foreach (TagCategory category in Tag.TAG_CATEGORIES) {
                    List<TagDTO> collection = [.. Gallery.Tags.Where(t => t.Category == category).OrderBy(t => t.Value)];
                    if (collection.Count > 0) {
                        _tagCollections.Add(new(category, collection));
                    }
                }
                ResizeListener.OnResized += OnResize;
                StateHasChanged();
            }
        }

        private void OnClick(MouseEventArgs e) {
            if (IsEditing && e.Button == 0) {
                IsSelected = !IsSelected;
                IsSelectedChanged.InvokeAsync(IsSelected);
            }
        }

        private void OnResize(object? sender, BrowserWindowSize size) {
            _ = SetMaxImageCount();
        }

        private async Task SetMaxImageCount() {
            int width = await JSRuntime.InvokeAsync<int>("getClientWidthById", _imageContainerId);
            double aspectRatio = (double)width / THUMBNAIL_IMAGE_HEIGHT;
            if (aspectRatio <= _maxRecordedAspectRatio) {
                return;
            }
            _maxRecordedAspectRatio = aspectRatio;
            if (aspectRatio >= _cumulativeImageAspectRatios[^1]) {
                _maxImageCount = Gallery.Images.Count;
            } else {
                for (int i = _maxImageCount - 1; i < _cumulativeImageAspectRatios.Length; i++) {
                    if (_cumulativeImageAspectRatios[i] > aspectRatio) {
                        _maxImageCount = i + 1;
                        break;
                    }
                }
            }
            StateHasChanged();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
            ResizeListener.OnResized -= OnResize;
        }
    }
}
