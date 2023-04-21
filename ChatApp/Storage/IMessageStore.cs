using ChatApp.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Storage
{
    public interface IMessageStore 
    {
        Task SendMessage(Message message);
        Task<(List<ListMessagesResponseItem>,string)> ListMessages(string conversationId, int limit, long lastSeenMessageTime,string continuationToken);
    }
}
