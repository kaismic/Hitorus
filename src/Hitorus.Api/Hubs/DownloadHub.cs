using Hitorus.Data;
using Hitorus.Data.DbContexts;
using Microsoft.AspNetCore.SignalR;

namespace Hitorus.Api.Hubs {
    public class DownloadHub(HitomiContext dbContext) : Hub<IDownloadClient> {
        public override Task OnConnectedAsync() {
            Clients.Caller.ReceiveCreateDownloads(dbContext.DownloadConfigurations.First().SavedDownloads);
            return base.OnConnectedAsync();
        }
    }
}
