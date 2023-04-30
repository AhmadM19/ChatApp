using ChatApp.Dtos;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Exceptions;

namespace ChatApp.Storage
{
    public interface IConversationStore 
    {
        ///<exception cref="ArgumentException">if arguments are not passed correctly</exception>
        ///<exception cref="DuplicateConversationException">if conversation already exists</exception>
        ///<exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task AddConversation(Conversation conversation);

        ///<exception cref="ArgumentException">if arguments are not passed correctly</exception>
        ///<exception cref="ConversationNotFoundException">if conversation doesn't exist</exception>
        ///<exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task UpdateConversation(Conversation conversation);

        ///<exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task<(List<ListConversationsResponseItemSchema>, string)> ListConversations(string username, int limit, long lastSeenConversationTime, string continuationToken);
        ///<exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task<Conversation?> GetConversation(string username, string conversationId);
        ///<exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task DeleteConversation(string username, string conversationId);
    }
}

