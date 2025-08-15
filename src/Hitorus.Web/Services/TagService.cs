using Blazored.LocalStorage;
using Hitorus.Data.Entities;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class TagService {
        private readonly HttpClient _httpClient;
        public TagService(HttpClient httpClient, IConfiguration hostConfiguration, ISyncLocalStorageService localStorageService) {
            _httpClient = httpClient;
            _httpClient.BaseAddress = Utilities.GetServiceBaseUri(hostConfiguration, localStorageService, "TagServicePath");
        }

        public async Task<IEnumerable<Tag>> GetTagsAsync(TagCategory category, int count, string? value, CancellationToken ct) {
            string valueQuery = value == null || value.Length == 0 ? "" : $"&value={value}";
            try {
                return (await _httpClient.GetFromJsonAsync<IEnumerable<Tag>>($"search?category={category}&count={count}{valueQuery}", ct))!;
            } catch (TaskCanceledException) {
                return [];
            }
        }
    }
}
