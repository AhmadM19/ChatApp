using System.ComponentModel.DataAnnotations;
namespace ChatApp.Dtos
{
 public record Profile(
     [Required] string username,
     [Required] string firstName,
     [Required] string lastName,
     string? profilePictureId=null);
}
