using Hitorus.Api.Download;
using Hitorus.Api.Hubs;
using Hitorus.Api.Services;
using Hitorus.Data;
using Hitorus.Data.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Hitorus.Api {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddDbContextFactory<HitomiContext>(options => {
                options.UseSqlite(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    optionsBuilder => {
                        optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }
                );
            });
            builder.Services.AddSignalR();
            builder.Services.AddCors(options => {
                options.AddPolicy("AllowAnyOrigin", builder =>
                    builder.AllowAnyOrigin()
                        .SetIsOriginAllowed(host => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        );
            });
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<TagUtilityService>();
            builder.Services.AddHostedService<DownloadManagerService>();
            builder.Services.AddSingleton<IEventBus<DownloadEventArgs>, DownloadEventBus>();

            var app = builder.Build();
            if (app.Environment.IsProduction()) {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseRouting();
            app.UseCors("AllowAnyOrigin");

            app.MapHub<DownloadHub>("api/download-hub");
            app.MapControllers();

            app.Run();
        }
    }
}
