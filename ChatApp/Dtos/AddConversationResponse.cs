using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Dtos
{
    public record AddConversationResponse(string Id, string[] Participants, DateTime LastModifiedDateUtc);
}

