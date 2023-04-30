namespace ChatApp.Exceptions
{
    public class DuplicateConversationException:Exception
    {
        public DuplicateConversationException(string? conversationId)
: base($"conversation with id {conversationId} already exists")
        {
        }
    }
}
