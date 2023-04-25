namespace ChatApp.Dtos
{
    public record ListMessagesResponseItem(string text, string senderUserName, long unixTime);
}
