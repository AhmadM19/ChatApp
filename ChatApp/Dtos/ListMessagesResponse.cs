namespace ChatApp.Dtos
{
    public record ListMessagesResponse(IEnumerable<ListMessagesResponseItem> messages, string nextUri);

}
