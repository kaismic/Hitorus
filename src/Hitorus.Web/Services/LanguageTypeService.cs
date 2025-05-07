using Hitorus.Data.DTOs;
using System.Net.Http.Json;

namespace Hitorus.Web.Services {
    public class LanguageTypeService(HttpClient httpClient) {
        public bool IsLoaded { get; private set; } = false;
        public List<GalleryTypeDTO> Types { get; private set; } = [];
        public List<GalleryLanguageDTO> Languages { get; private set; } = [];

        public async Task Load() {
            Languages = (await httpClient.GetFromJsonAsync<List<GalleryLanguageDTO>>("languages"))!;
            Types = (await httpClient.GetFromJsonAsync<List<GalleryTypeDTO>>("types"))!;
            IsLoaded = true;
        }
    }
}
