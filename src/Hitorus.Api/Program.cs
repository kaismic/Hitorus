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
            //builder.Services.AddDbContext<ApplicationDbContext>();
            builder.Services.AddSignalR();
            string webAppUrl = builder.Configuration["WebAppUrl"]!;
            builder.Services.AddCors(options => {
                options.AddPolicy("AllowLocalhostOrigins", builder =>
                    builder.WithOrigins(webAppUrl)
                        .SetIsOriginAllowed(host => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        //.AllowCredentials()
                        );
            });
            //builder.Services.AddAuthorization();
            //builder.Services.AddIdentityApiEndpoints<IdentityUser>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddHttpClient();
            builder.Services.AddScoped<TagUtilityService>();
            if (builder.Environment.IsDevelopment()) {
                builder.Services.AddHostedService<DbInitializeService>();
            } else {
                DbInitializeService.IsInitialized = true;
            }
            builder.Services.AddHostedService<DownloadManagerService>();
            builder.Services.AddSingleton<IEventBus<DownloadEventArgs>, DownloadEventBus>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.MapOpenApi();
            } else {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            //app.UseStaticFiles();
            // app.UseCookiePolicy();

            app.UseRouting();
            // app.UseRateLimiter();
            // app.UseRequestLocalization();
            app.UseCors("AllowLocalhostOrigins");

            //app.UseAuthentication();
            //app.UseAuthorization();
            // app.UseSession();
            // app.UseResponseCompression(

            app.MapHub<DbStatusHub>("api/db-status-hub");
            app.MapHub<DownloadHub>("api/download-hub");
            app.MapControllers();

            //app.MapIdentityApi<IdentityUser>();

            //if (app.Environment.IsDevelopment()) {
            //    app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
            //        string.Join("\n", endpointSources.SelectMany(source => source.Endpoints)));
            //}
            app.Run();
        }
    }
}
