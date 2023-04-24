using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Dtos
{
    public record SendMessageRequest([Required] string id, [Required]  string text, [Required] string senderUsername);
}
