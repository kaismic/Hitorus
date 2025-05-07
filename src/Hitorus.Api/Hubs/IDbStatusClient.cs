using Hitorus.Data;

namespace Hitorus.Api.Hubs {
    public interface IDbStatusClient {
        Task ReceiveStatus(DbInitStatus status, string message);
    }
}
