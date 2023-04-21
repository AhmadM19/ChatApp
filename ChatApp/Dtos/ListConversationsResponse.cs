namespace ChatApp.Dtos
{
    public record ListConversationsResponse(List<ListConversationsResponseItem> conversations, string nextUri);
}
