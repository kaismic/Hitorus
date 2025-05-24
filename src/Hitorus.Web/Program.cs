using Blazored.LocalStorage;
using BlazorPro.BlazorSize;
using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace Hitorus.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddMudServices();
        builder.Services.AddLocalization(options => options.ResourcesPath = "Localization");
        builder.Services.AddHttpClient();
        builder.Services.AddBlazoredLocalStorageAsSingleton();
        builder.Services.AddResizeListener(options => options.ReportRate = 500);
        builder.Services.AddSingleton<SearchConfigurationService>();
        builder.Services.AddSingleton<BrowseConfigurationService>();
        builder.Services.AddSingleton<DownloadConfigurationService>();
        builder.Services.AddSingleton<ViewConfigurationService>();
        builder.Services.AddSingleton<AppConfigurationService>();
        builder.Services.AddSingleton<LanguageTypeService>();
        builder.Services.AddSingleton<TagFilterService>();
        builder.Services.AddSingleton<SearchFilterService>();
        builder.Services.AddSingleton<TagService>();
        builder.Services.AddSingleton<GalleryService>();
        builder.Services.AddSingleton<DownloadService>();
        builder.Services.AddSingleton<DownloadClientManagerService>();
        await builder.Build().RunAsync();
    }
}
