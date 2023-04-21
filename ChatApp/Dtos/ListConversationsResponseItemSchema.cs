namespace ChatApp.Dtos
{
    public record ListConversationsResponseItemSchema(string id, string participant, long lastModifiedUnixTime);

}
