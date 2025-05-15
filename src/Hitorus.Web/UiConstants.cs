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

    // Helper websites
    // https://coolors.co/palettes/popular
    // https://m2.material.io/design/color/the-color-system.html#tools-for-picking-colors
    private const string COLOR_PRIMARY_LIGHT = "#263238";
    private const string COLOR_PRIMARY_DARK = "#455A64";
    public static readonly MudTheme APP_THEME = new() {
        PaletteLight = new() {
            Primary = COLOR_PRIMARY_LIGHT,
            Secondary = "#770a00",
            Tertiary = "#5c005f",
            AppbarBackground = COLOR_PRIMARY_LIGHT
        },
        PaletteDark = new() {
            Primary = COLOR_PRIMARY_DARK,
            Secondary = "#a32116",
            Tertiary = "#960f7b",
            AppbarBackground = COLOR_PRIMARY_DARK
        }
    };
}
