namespace ChatApp.Exceptions
{
    public class MessageNotFoundException:Exception
    {
        public MessageNotFoundException(string? conversationId)
    : base($"No messages found in conversation with conversationId {conversationId}")
        {
        }
    }
}
