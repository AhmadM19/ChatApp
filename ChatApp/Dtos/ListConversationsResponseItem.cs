namespace ChatApp.Dtos
{
    public record ListConversationsResponseItem(string id, Profile recipient, long lastModifiedUnixTime);

}
