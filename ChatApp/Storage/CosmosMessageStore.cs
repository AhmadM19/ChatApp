using ChatApp.Configuration;
using ChatApp.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Storage.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
//using System.ComponentModel;
using System.Net;

namespace ChatApp.Storage
{
    public class CosmosMessageStore : IMessageStore
    {
        private readonly CosmosClient _cosmosClient;

        public CosmosMessageStore(IOptions<CosmosSettings> options)
        {
            _cosmosClient = new CosmosClient(options.Value.ConnectionString);
        }

        private Container Container => _cosmosClient.GetDatabase("chatapp").GetContainer("message");
        private static MessageEntity ToEntity(Message message)
        {
            return new MessageEntity(
                partitionKey: message.conversationId,
                id: message.messageId,
                message.senderUsername,
                message.text,
                message.createdUnixTime);
        }
        private static Message ToMessage(MessageEntity entity)
        {
            return new Message(
                conversationId: entity.partitionKey,
                messageId: entity.id,
                senderUsername: entity.senderUsername,
                text: entity.text,
                createdUnixTime: entity.createdUnixTime);
        }


        public async Task SendMessage(Message message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.conversationId) || string.IsNullOrWhiteSpace(message.messageId)
                || string.IsNullOrWhiteSpace(message.senderUsername) || string.IsNullOrWhiteSpace(message.text) || string.IsNullOrWhiteSpace(message.createdUnixTime.ToString()))
            {
                throw new ArgumentException($"Invalid message{message}", nameof(message));
            }
            await Container.CreateItemAsync(ToEntity(message));
        }

        public async Task<(List<ListMessagesResponseItem>, string)> ListMessages(string conversationId, int limit, long lastSeenMessageTime, string continuationToken)
        {
            var messages = new List<ListMessagesResponseItem>();
            var queryOptions = new QueryRequestOptions() { MaxItemCount=limit, PartitionKey = new PartitionKey(conversationId) };

            QueryDefinition queryDefinition = new QueryDefinition("SELECT m.text, m.senderUsername, m.createdUnixTime AS unixTime "+
                "FROM m WHERE m.partitionKey=@conversationId " +
                "AND m.createdUnixTime > @lastSeenMessageTime " +
                "ORDER BY m.createdUnixTime DESC ")
                .WithParameter("@conversationId", conversationId)
                .WithParameter("@lastSeenMessageTime", lastSeenMessageTime);

            FeedIterator<ListMessagesResponseItem> feedIterator = Container.GetItemQueryIterator<ListMessagesResponseItem>(queryDefinition,
                continuationToken:continuationToken,
                requestOptions: queryOptions );

            var responseToken = "";
            while (feedIterator.HasMoreResults && messages.Count < limit)
            {
                FeedResponse<ListMessagesResponseItem> response = await feedIterator.ReadNextAsync();
                messages.AddRange(response.ToList());
                responseToken = response.ContinuationToken;
            }
            return (messages,responseToken);
            
        }


        /*    public async Task DeleteProfile(string username)
   {
       try
       {
           await Container.DeleteItemAsync<Profile>(
               id: username, partitionKey: new PartitionKey(username)
           );
       }
       catch (CosmosException e)
       {
           if (e.StatusCode == HttpStatusCode.NotFound) { return; }
           throw;
       }
   }
}
} */
    }
}
