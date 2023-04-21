using ChatApp.Dtos;
using ChatApp.Services;
using ChatApp.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class imagesController:ControllerBase
    {
        private readonly IImageService _imageService;
        public imagesController(IImageService imageService)
        {
            _imageService= imageService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadImage(string id)
        {
            var imageData = await _imageService.DownloadImage(id);
            if (imageData == null)
            {
                return NotFound($"The image with id {id} is not found");
            }
            return File(imageData, "image/png");
        }

        [HttpPost]
        public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
        {
            using var stream = new MemoryStream();
            await request.File.CopyToAsync(stream);
            var imageId = await _imageService.UploadImage(stream.ToArray());
            return CreatedAtAction(nameof(DownloadImage), new { id = imageId },new UploadImageResponse(imageId));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(string id)
        { 
            await _imageService.DeleteImage(id);
            return Ok();
            
        }

    }
}
