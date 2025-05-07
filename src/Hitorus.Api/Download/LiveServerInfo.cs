namespace Hitorus.Api.Download {
    public class LiveServerInfo {
        public int ServerTime { get; init; }
        public HashSet<string> SubdomainSelectionSet { get; init; } = [];
        public bool IsContains { get; init; }
    }
}
