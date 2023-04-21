namespace ChatApp.Storage.Entities
{
    public record MessageEntity(string partitionKey, string id, string senderUsername, string text, long createdUnixTime);
}
