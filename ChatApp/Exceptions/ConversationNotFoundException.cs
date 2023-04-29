namespace ChatApp.Exceptions
{
    public class ConversationNotFoundException : Exception
    {
        public ConversationNotFoundException(string? conversationId)
            : base($"conversation with id {conversationId} doesn't exist")
        {
        }
    }
}
