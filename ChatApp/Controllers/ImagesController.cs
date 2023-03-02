using ChatApp.Dtos;
using ChatApp.Storage;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImagesController:ControllerBase
    {
        private readonly IImageStore _imageStore;
        public ImagesController(BlobImageStore imageStore)
        {
            _imageStore = imageStore;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FileContentResult>> DownloadImage(string id)
        {
            var image = await _imageStore.DownloadImage(id);
            if (image == null)
            {
                return NotFound($"The image with id {id} is not found");
            }
            return Ok(image);
        }
        [HttpPost]
        public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
        { 
            var response= await _imageStore.UploadImage(request.File);
            var UploadImageResponse = new UploadImageResponse(response);
            return CreatedAtAction(nameof(UploadImage), new { imageId = UploadImageResponse.imageId }, UploadImageResponse);

        }

    }
}
