using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Dtos
{
    public record AddConversationRequest( SendMessageRequest firstMessage,params string[] participants);
}
