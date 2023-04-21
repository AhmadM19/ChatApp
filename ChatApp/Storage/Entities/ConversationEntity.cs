namespace ChatApp.Storage.Entities
{
    public record ConversationEntity(string partitionKey, string id, string participant, long lastModifiedUnixTime);
}
