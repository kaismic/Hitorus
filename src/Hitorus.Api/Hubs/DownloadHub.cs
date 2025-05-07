using Hitorus.Data.DbContexts;
using Microsoft.AspNetCore.SignalR;

namespace Hitorus.Api.Hubs {
    public class DownloadHub(HitomiContext dbContext) : Hub<IDownloadClient> {
        public override Task OnConnectedAsync() {
            Clients.Caller.ReceiveSavedDownloads(dbContext.DownloadConfigurations.First().Downloads);
            return base.OnConnectedAsync();
        }
    }
}
