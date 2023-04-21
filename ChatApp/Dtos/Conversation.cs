using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Dtos
{
    public record Conversation([Required] string username, [Required] string participant, [Required] string conversationId,
        [Required] long lastModifiedUnixTime);
}
