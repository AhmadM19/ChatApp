using ChatApp.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Storage
{
    public interface IConversationStore 
    {
       Task AddConversation(Conversation conversation);

       Task UpdateConversation(Conversation conversation);
       Task<(List<ListConversationsResponseItemSchema>, string)> ListConversations(string username, int limit, long lastSeenConversationTime, string continuationToken);
    }
}

