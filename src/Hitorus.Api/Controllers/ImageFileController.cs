using Hitorus.Api.Utilities;
using Hitorus.Data.DbContexts;
using Hitorus.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Hitorus.Api.Controllers {
    [ApiController]
    [Route("api/image")]
    public class ImageFileController(HitomiContext context) : ControllerBase {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult GetImage(int galleryId, int index) {
            Gallery? gallery = context.Galleries.Find(galleryId);
            if (gallery == null) {
                return NotFound($"Gallery with id {galleryId} was not found.");
            }
            int imageCount = context.Entry(gallery).Collection(g => g.Images).Query().Count();
            if (index < 1 || index > imageCount) {
                return BadRequest($"Image index {index} is out of range.");
            }
            context.Entry(gallery).Collection(g => g.Images).Load();
            GalleryImage? image = gallery.Images.FirstOrDefault(gi => gi.Index == index);
            if (image == null) {
                return NotFound($"Image information at the index {index} was not found.");
            }
            try {
                string path = GalleryFileUtility.GetImagePath(gallery, image);
                FileStream stream = System.IO.File.OpenRead(path);
                return File(stream, $"image/{path.Split('.').Last()}");
            } catch (FileNotFoundException) {
                return NotFound($"Image file at the index {index} was not found.");
            }
        }
    }
}
