using System.ComponentModel.DataAnnotations;
namespace ChatApp.Dtos
{
    public record UploadImageRequest( [Required] IFormFile File);
}
