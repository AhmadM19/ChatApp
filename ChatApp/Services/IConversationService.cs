using ChatApp.Dtos;

namespace ChatApp.Services
{
    public interface IConversationService
    {
        Task<long> SendMessage(string conversationId, string messageId, string senderUsername, string text);
        Task<string[]> AddConversation(SendMessageRequest firstMessage,params string[] participants);
        Task<(List<ListMessagesResponseItem>,string)> ListMessages(string conversationId, int limit, long lastSeenMessageTime,string continuationToken);
        Task<(List<ListConversationsResponseItem>, string)> ListConversations(string username, int limit, long lastSeenConversationTime,string continuationToken);
    }
}
