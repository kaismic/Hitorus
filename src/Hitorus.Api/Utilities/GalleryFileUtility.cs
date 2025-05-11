using Hitorus.Data.Entities;
using System.Text.RegularExpressions;

namespace Hitorus.Api.Utilities {
    public static partial class GalleryFileUtility {
        private const string ROOT_PATH = "Galleries";
        [GeneratedRegex(@"\d+")] private static partial Regex AllDigitRegex();

        public static IEnumerable<GalleryImage> GetMissingImages(int galleryId, IEnumerable<GalleryImage> galleryImages) {
            string dir = Path.Combine(ROOT_PATH, galleryId.ToString());
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
                return galleryImages;
            }
            HashSet<int> existingIndexes =
                [.. Directory.GetFiles(dir, "*.*")
                .Select(Path.GetFileName)
                .Cast<string>()
                .Select(f => f.Split('.')[0])
                .Where(name => AllDigitRegex().IsMatch(name))
                .Select(int.Parse)];
            return galleryImages.Where(gi => !existingIndexes.Contains(gi.Index));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gallery"></param>
        /// <param name="galleryImage"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static string GetImagePath(Gallery gallery, GalleryImage galleryImage) {
            string[] fullFilePaths = Directory.GetFiles(Path.Combine(ROOT_PATH, gallery.Id.ToString()), "*.*");
            foreach (string fullFilePath in fullFilePaths) {
                string fileName = Path.GetFileName(fullFilePath);
                Regex regex = new($@"0*{galleryImage.Index}\.(avif|webp)");
                if (regex.IsMatch(fileName)) {
                    return fullFilePath;
                }
            }
            throw new FileNotFoundException();
        }

        public static async Task WriteImageAsync(Gallery gallery, GalleryImage galleryImage, byte[] data, string fileExt) {
            string format = "D" + Math.Floor(Math.Log10(gallery.Images.Count) + 1);
            string fileName = galleryImage.Index.ToString(format);
            string fullFileName = fileName + '.' + fileExt;
            string dir = Path.Combine(ROOT_PATH, gallery.Id.ToString());
            Directory.CreateDirectory(dir);
            await File.WriteAllBytesAsync(Path.Combine(dir, fullFileName), data);
        }

        public static void DeleteGalleryDir(int id) {
            string dir = Path.Combine(ROOT_PATH, id.ToString());
            if (Directory.Exists(dir)) {
                Directory.Delete(dir, true);
            }
        }
    }
}
