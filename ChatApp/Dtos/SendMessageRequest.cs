using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Dtos
{
    public record SendMessageRequest([Required] string messageId, [Required]  string senderUsername, [Required] string text);
}
