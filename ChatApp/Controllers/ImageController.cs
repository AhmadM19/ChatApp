using ChatApp.Dtos;
using ChatApp.Exceptions;
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
            try 
            { 
                var imageData= await _imageService.DownloadImage(id);
                return File(imageData, "image/png");
            }
            catch(ImageNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
        {
            try
            {
                using var stream = new MemoryStream();
                await request.File.CopyToAsync(stream);
                var imageId = await _imageService.UploadImage(stream.ToArray());
                return CreatedAtAction(nameof(DownloadImage), new { id = imageId }, new UploadImageResponse(imageId));
            }
            catch(ArgumentNullException e)
            {
                return BadRequest(e.Message);
            }
            //No Conflict case since every new image has a unique generated Id
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(string id)
        {
            try
            {
                await _imageService.DeleteImage(id);
                return Ok();
            }
            catch(ImageNotFoundException e)
            {
                return NotFound(e.Message);
            }     
        }
    }
}
