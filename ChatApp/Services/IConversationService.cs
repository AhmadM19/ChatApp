using ChatApp.Dtos;
using ChatApp.Exceptions;

namespace ChatApp.Services
{
    public interface IConversationService
    {
        ///<exception cref="ProfileNotFoundException">if the username passed does not exists</exception>
        ///<exception cref="ConversationNotFoundException">if conversation trying to add to doesn't exist</exception>
        ///<exception cref="DuplicateMessageException">if message already exists</exception>
        ///<exception cref="ArgumentException">if arguments are not passed correctly</exception>
        ///<exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task<long> SendMessage(string conversationId, string messageId, string senderUsername, string text);

        ///<exception cref="ProfileNotFoundException">if the usernames of participants passed does not exists</exception>
        ///<exception cref="DuplicateMessageException">if first message Id being added already exists</exception>
        ///<exception cref="DuplicateConversationException">if conversation being created already exists</exception>
        ///<exception cref="ArgumentException">if arguments are not passed correctly</exception>
        ///<exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task<(string conversationId, long CreatedUnixTime)> AddConversation(SendMessageRequest firstMessage,params string[] participants);

        ///<exception cref="MessageNotFoundException">if no messages found in the given conversation</exception>
        ///<exception cref="ArgumentException">if arguments are not passed correctly</exception>
        ///<exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task<(List<ListMessagesResponseItem>,string)> ListMessages(string conversationId, int limit, long lastSeenMessageTime,string continuationToken);

        ///<exception cref="ConversationNotFoundException">if no conversations exist for the given user</exception>
        ///<exception cref="ArgumentException">if arguments are not passed correctly</exception>
        ///<exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task<(List<ListConversationsResponseItem>, string)> ListConversations(string username, int limit, long lastSeenConversationTime,string continuationToken);
    }
}
