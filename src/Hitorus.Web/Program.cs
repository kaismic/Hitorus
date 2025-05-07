using Hitorus.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using BlazorPro.BlazorSize;

namespace Hitorus.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddMudServices();
        //builder.Services.AddLocalization();

        string apiUrl = builder.Configuration["ApiUrl"]!;
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton(sp =>
            {
                HttpClient httpClient = sp.GetRequiredService<HttpClient>();
                httpClient.BaseAddress = new Uri(apiUrl + builder.Configuration["SearchConfigPath"]);
                return new SearchConfigurationService(httpClient);
            }
        );
        builder.Services.AddSingleton(sp =>
            {
                HttpClient httpClient = sp.GetRequiredService<HttpClient>();
                httpClient.BaseAddress = new Uri(apiUrl + builder.Configuration["BrowseConfigPath"]);
                return new BrowseConfigurationService(httpClient);
            }
        );
        builder.Services.AddSingleton(sp =>
            {
                HttpClient httpClient = sp.GetRequiredService<HttpClient>();
                httpClient.BaseAddress = new Uri(apiUrl + builder.Configuration["DownloadConfigPath"]);
                return new DownloadConfigurationService(httpClient);
            }
        );
        builder.Services.AddSingleton(sp =>
            {
                HttpClient httpClient = sp.GetRequiredService<HttpClient>();
                httpClient.BaseAddress = new Uri(apiUrl + builder.Configuration["ViewConfigPath"]);
                return new ViewConfigurationService(httpClient);
            }
        );
        builder.Services.AddSingleton(sp =>
            {
                HttpClient httpClient = sp.GetRequiredService<HttpClient>();
                httpClient.BaseAddress = new Uri(apiUrl + builder.Configuration["AppConfigPath"]);
                return new AppConfigurationService(httpClient, builder.Configuration);
            }
        );
        builder.Services.AddSingleton(sp =>
            {
                HttpClient httpClient = sp.GetRequiredService<HttpClient>();
                httpClient.BaseAddress = new Uri(apiUrl + builder.Configuration["LanguageTypePath"]);
                return new LanguageTypeService(httpClient);
            }
        );
        builder.Services.AddSingleton(sp =>
            {
                HttpClient httpClient = sp.GetRequiredService<HttpClient>();
                httpClient.BaseAddress = new Uri(apiUrl + builder.Configuration["TagFilterPath"]);
                return new TagFilterService(httpClient, sp.GetRequiredService<SearchConfigurationService>());
            }
        );
        builder.Services.AddSingleton(sp =>
            {
                HttpClient httpClient = sp.GetRequiredService<HttpClient>();
                httpClient.BaseAddress = new Uri(apiUrl + builder.Configuration["SearchFilterPath"]);
                return new SearchFilterService(httpClient, sp.GetRequiredService<SearchConfigurationService>());
            }
        );
        builder.Services.AddSingleton(sp =>
            {
                HttpClient httpClient = sp.GetRequiredService<HttpClient>();
                httpClient.BaseAddress = new Uri(apiUrl + builder.Configuration["TagPath"]);
                return new TagService(httpClient);
            }
        );
        builder.Services.AddSingleton(sp =>
            {
                HttpClient httpClient = sp.GetRequiredService<HttpClient>();
                httpClient.BaseAddress = new Uri(apiUrl + builder.Configuration["GalleryPath"]);
                return new GalleryService(httpClient);
            }
        );
        builder.Services.AddSingleton(sp =>
            {
                HttpClient httpClient = sp.GetRequiredService<HttpClient>();
                httpClient.BaseAddress = new Uri(apiUrl + builder.Configuration["DownloadServicePath"]);
                return new DownloadService(httpClient);
            }
        );
        builder.Services.AddSingleton<DownloadClientManagerService>();
        builder.Services.AddResizeListener(options => options.ReportRate = 500);
        var app = builder.Build();
        await app.RunAsync();
    }
}
