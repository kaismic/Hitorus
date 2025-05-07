using Hitorus.Data.Entities;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class TagService(HttpClient httpClient) {
        public async Task<IEnumerable<Tag>> GetTagsAsync(TagCategory category, int count, string? start, CancellationToken ct) {
            string startStr = start == null || start.Length == 0 ? "" : $"&start={start}";
            try {
                return (await httpClient.GetFromJsonAsync<IEnumerable<Tag>>($"search?category={category}&count={count}{startStr}", ct))!;
            } catch (TaskCanceledException) {
                return [];
            }
        }
    }
}
