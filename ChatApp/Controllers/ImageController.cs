using ChatApp.Dtos;
using ChatApp.Exceptions;
using ChatApp.Services;
using ChatApp.Storage;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic.FileIO;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class imagesController:ControllerBase
    {
        private readonly IImageService _imageService;
        private readonly ILogger<imagesController> _logger;
        TelemetryClient _telemetryClient;
        public imagesController(IImageService imageService, ILogger<imagesController> logger, TelemetryClient telemetryClient)
        {
            _imageService= imageService;
            _logger= logger;
            _telemetryClient= telemetryClient;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadImage(string id)
        {
            using (_logger.BeginScope("{ImageId}", id))
            {
                try
                {
                    _logger.LogInformation("Downloading Image with id {ImageId}");
                    var imageData = await _imageService.DownloadImage(id);
                    return File(imageData, "image/png");
                }
                catch (ImageNotFoundException e)
                {
                    _logger.LogWarning("Image with id {ImageId} was not found");
                    return NotFound(e.Message);
                }
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
                using (_logger.BeginScope("{ImageId}", imageId))
                {
                    _logger.LogInformation("Uploading Image with id {ImageId}");
                }
                return CreatedAtAction(nameof(DownloadImage), new { id = imageId }, new UploadImageResponse(imageId));
            }
            catch(ArgumentNullException e)
            {
                _logger.LogWarning(e.Message);
                return BadRequest(e.Message);
            }
            //No Conflict case since every new image has a unique generated Id
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(string id)
        {
            using (_logger.BeginScope("{ImageId}", id))
            {
                try
                {
                    _logger.LogInformation("Deleting image with id {ImageId}");
                    await _imageService.DeleteImage(id);
                    return Ok();
                }
                catch (ImageNotFoundException e)
                {
                    _logger.LogWarning("Image with id {ImageId} was not found");
                    return NotFound(e.Message);
                }
            }
        }
    }
}
