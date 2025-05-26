using Hitorus.Api.Download;
using Hitorus.Api.Hubs;
using Hitorus.Api.Services;
using Hitorus.Data;
using Hitorus.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

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
            if (appBuilder.Environment.IsDevelopment()) {
                appBuilder.Services.AddCors(options => {
                    options.AddPolicy("HitorusCorsPolicy", corsPolicyBuilder =>
                        corsPolicyBuilder.WithOrigins("https://localhost")
                            .SetIsOriginAllowed(host => true)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            );
                });
            } else {
                appBuilder.Services.AddCors(options => {
                    options.AddPolicy("HitorusCorsPolicy", corsPolicyBuilder =>
                        corsPolicyBuilder.SetIsOriginAllowedToAllowWildcardSubdomains()
                            .WithOrigins(appBuilder.Configuration["AllowedOrigins"]!.Split(';'))
                            .SetIsOriginAllowed(origin => true)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            );
                });
            }
            appBuilder.Services.AddHttpClient();
            appBuilder.Services.AddScoped<TagUtilityService>();
            appBuilder.Services.AddHostedService<DownloadManagerService>();
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

            app.Run();
        }
    }
}
