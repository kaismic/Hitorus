using Hitorus.Data.Entities;
using System.Text.RegularExpressions;

namespace Hitorus.Api.Utilities {
    public static partial class GalleryIOUtility {
        private const string ROOT_PATH = "Galleries";
        [GeneratedRegex(@".*?(\d{6,7})")] private static partial Regex ContainsIdRegex();

        public static IEnumerable<GalleryImage> GetMissingImages(int galleryId, IEnumerable<GalleryImage> galleryImages) {
            string galleryDirName = GetGalleryDirectoryName(galleryId) ?? galleryId.ToString();
            string dir = Path.Combine(ROOT_PATH, galleryDirName);
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
                return galleryImages;
            }
            HashSet<int> existingIndexes =
                [.. Directory.GetFiles(dir, "*.*")
                .Select(Path.GetFileName)
                .Cast<string>()
                .Select(f => f.Split('.')[0])
                .Where(name => int.TryParse(name, out _))
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
            string galleryDirName = GetGalleryDirectoryName(gallery.Id) ??
                throw new DirectoryNotFoundException($"Gallery directory for ID {gallery.Id} not found.");
            string[] fullFilePaths = Directory.GetFiles(Path.Combine(ROOT_PATH, galleryDirName), "*.*");
            foreach (string fullFilePath in fullFilePaths) {
                string fileName = Path.GetFileName(fullFilePath);
                Regex regex = new($@"0*{galleryImage.Index}\.+");
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
            string galleryDirName = GetGalleryDirectoryName(gallery.Id) ?? gallery.Id.ToString();
            string dir = Path.Combine(ROOT_PATH, galleryDirName);
            Directory.CreateDirectory(dir);
            await File.WriteAllBytesAsync(Path.Combine(dir, fullFileName), data);
        }

        public static void DeleteGalleryDirectory(int id) {
            string galleryDirName = GetGalleryDirectoryName(id) ?? id.ToString();
            string dir = Path.Combine(ROOT_PATH, galleryDirName);
            if (Directory.Exists(dir)) {
                Directory.Delete(dir, true);
            }
        }

        public static void RenameDirectory(int oldId, int newId) {
            string galleryDirName = GetGalleryDirectoryName(oldId) ??
                throw new DirectoryNotFoundException($"Gallery directory for ID {oldId} not found.");
            string oldDir = Path.Combine(ROOT_PATH, galleryDirName);
            if (Directory.Exists(oldDir)) {
                string newDir = Path.Combine(ROOT_PATH, galleryDirName.Replace(oldId.ToString(), newId.ToString()));
                if (Directory.Exists(newDir)) {
                    Directory.Delete(oldDir, true);
                } else {
                    Directory.Move(oldDir, newDir);
                }
            }
        }

        private static string? GetGalleryDirectoryName(int id) {
            if (!Directory.Exists(ROOT_PATH)) {
                return null;
            }
            return Directory.GetDirectories(ROOT_PATH)
                .Select(dir => Path.GetFileName(dir))
                .Where(name => name.Contains(id.ToString()))
                .FirstOrDefault();
        }

        public static IEnumerable<int> GetExistingGalleries() {
            if (!Directory.Exists(ROOT_PATH)) {
                return [];
            }
            return Directory.GetDirectories(ROOT_PATH)
                .Select(dir => Path.GetFileName(dir))
                .Where(name => ContainsIdRegex().IsMatch(name))
                .Select(name => ContainsIdRegex().Match(name).Groups[1].Value)
                .Select(int.Parse);
        }
    }
}
