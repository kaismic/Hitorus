using Hitorus.Api.Download;
using Hitorus.Api.Hubs;
using Hitorus.Api.Services;
using Hitorus.Data;
using Hitorus.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Hitorus.Api {
    public class Program {
        public static void Main(string[] args) {
            var appBuilder = WebApplication.CreateBuilder(args);

            appBuilder.Services.AddControllers();
            appBuilder.Services.AddDbContextFactory<HitomiContext>(options => {
                options.UseSqlite(
                    appBuilder.Configuration.GetConnectionString("DefaultConnection"),
                    optionsBuilder => {
                        optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }
                );
            });
            appBuilder.Services.AddSignalR();
            appBuilder.Services.AddCors(options => {
                options.AddPolicy("HitorusCorsPolicy", corsPolicyBuilder => {
                    if (appBuilder.Environment.IsDevelopment()) {
                        corsPolicyBuilder.WithOrigins("https://localhost");
                    }
                    corsPolicyBuilder.SetIsOriginAllowedToAllowWildcardSubdomains()
                        .WithOrigins(appBuilder.Configuration["AllowedOrigins"]!.Split(';'))
                        .SetIsOriginAllowed(origin => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            appBuilder.Services.AddHttpClient();
            appBuilder.Services.AddScoped<TagUtilityService>();
            appBuilder.Services.AddSingleton<IDownloadManagerService, DownloadManagerService>();
            appBuilder.Services.AddHostedService(provider => (DownloadManagerService)provider.GetRequiredService<IDownloadManagerService>());
            appBuilder.Services.AddSingleton<IEventBus<DownloadEventArgs>, DownloadEventBus>();
            appBuilder.Services.AddLocalization(options => options.ResourcesPath = "Localization");

            string[] supportedCultures = appBuilder.Configuration.GetSection("SupportedCultures").Get<string[]>()!;

            var app = appBuilder.Build();
            if (app.Environment.IsProduction()) {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseRouting();
            app.UseCors("HitorusCorsPolicy");
            app.UseRequestLocalization(supportedCultures);

            app.MapHub<DownloadHub>("api/download-hub");
            app.MapControllers();

            Process.Start(
                new ProcessStartInfo {
                    UseShellExecute = true,
                    FileName = appBuilder.Configuration["WebAppUrl"]
                }
            );

            app.Run();
        }
    }
}
