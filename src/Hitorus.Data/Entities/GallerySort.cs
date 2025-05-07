using Hitorus.Data.DTOs;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace Hitorus.Data.Entities {
    public enum GalleryProperty {
        Id, Title, UploadTime, LastDownloadTime, Type
    }
    [Index(nameof(IsActive))]
    [Index(nameof(RankIndex))]
    public class GallerySort {
        public int Id { get; private set; }
        public required GalleryProperty Property { get; init; }
        public required SortDirection SortDirection { get; set; }
        public bool IsActive { get; set; }
        public int RankIndex { get; set; } // lower index means higher rank/priority. Value ranges from 0 to number of GalleryPropertys - 1

        public GallerySortDTO ToDTO() => new() {
            Property = Property,
            SortDirection = SortDirection,
            IsActive = IsActive,
            RankIndex = RankIndex
        };
    }
}
