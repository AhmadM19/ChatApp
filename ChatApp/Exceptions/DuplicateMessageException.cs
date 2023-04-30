namespace ChatApp.Exceptions
{
    public class DuplicateMessageException : Exception
    {
        public DuplicateMessageException(string? messageId)
    : base($"message with id {messageId} already exists")
        {
        }
    }
}
