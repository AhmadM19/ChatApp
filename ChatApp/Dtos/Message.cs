using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.Dtos
{

    public record Message([Required] string conversationId,[Required] string messageId,
        [Required] string senderUsername, [Required] string text, [Required] long createdUnixTime);
}
