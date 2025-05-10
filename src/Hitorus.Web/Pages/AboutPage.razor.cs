using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Hitorus.Web.Pages {
    public partial class AboutPage {
        [Inject] IStringLocalizer<SharedResource> SharedLocalizer { get; set; } = default!;
        [Inject] IStringLocalizer<AboutPage> Localizer { get; set; } = default!;
    }
}
