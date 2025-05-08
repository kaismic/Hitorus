using MudBlazor;

namespace Hitorus.Web;
public class UiConstants {
    public static readonly Action<SnackbarOptions> DEFAULT_SNACKBAR_OPTIONS = options => {
        options.ShowCloseIcon = true;
        options.CloseAfterNavigation = true;
        options.ShowTransitionDuration = 0;
        options.HideTransitionDuration = 500;
        options.VisibleStateDuration = 3000;
        options.DuplicatesBehavior = SnackbarDuplicatesBehavior.Allow;
    };
}
