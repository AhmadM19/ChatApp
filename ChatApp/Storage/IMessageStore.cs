using ChatApp.Dtos;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Exceptions;

namespace ChatApp.Storage
{
    public interface IMessageStore 
    {
        ///<exception cref="DuplicateMessageException">if message already exists</exception>
        ///<exception cref="ArgumentException">if arguments are not passed correctly</exception>
        ///<exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task SendMessage(Message message);

        ///<exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task<(List<ListMessagesResponseItem>,string)> ListMessages(string conversationId, int limit, long lastSeenMessageTime,string continuationToken);

        ///<exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task<Message?> GetMessage(string conversationId, string messageId);

        ///<exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task DeleteMessage(string conversationId, string messageId);
    }
}
