using System.ComponentModel.DataAnnotations;
namespace ChatApp.Dtos
{
    public record UploadImageResponse([Required] string imageId);
}
