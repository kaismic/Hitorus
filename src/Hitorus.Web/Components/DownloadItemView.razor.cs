using Hitorus.Data;
using Hitorus.Web.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace Hitorus.Web.Components {
    public partial class DownloadItemView : ComponentBase {
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        //[Inject] DownloadService DownloadService { get; set; } = default!;
        [Parameter, EditorRequired] public DownloadItemViewModel ViewModel { get; set; } = default!;
        [Parameter, EditorRequired] public EventCallback<int> OnActionButtonClick { get; set; }
        [Parameter, EditorRequired] public EventCallback<int> OnDeleteButtonClick { get; set; }

        private const string START_DELETE_ANIM_JS_FUNC = "startDeleteAnimation";

        private string ControlButtonIcon => ViewModel.Status switch {
            DownloadStatus.Downloading => MudBlazor.Icons.Material.Filled.Pause,
            DownloadStatus.Completed => MudBlazor.Icons.Material.Filled.Check,
            DownloadStatus.Paused or DownloadStatus.Failed or DownloadStatus.Enqueued => MudBlazor.Icons.Material.Filled.PlayArrow,
            DownloadStatus.Deleted => "",
            _ => throw new NotImplementedException()
        };

        protected override void OnInitialized() {
            //ViewModel.StateHasChanged = StateHasChanged;
            ViewModel.StartDeleteAnimation = StartDeleteAnimation;
        }

        private async ValueTask StartDeleteAnimation(string elementId, int animDuration) {
            await JSRuntime.InvokeVoidAsync(START_DELETE_ANIM_JS_FUNC, elementId, animDuration);
        }
    }
}