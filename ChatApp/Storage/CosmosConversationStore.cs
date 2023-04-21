using ChatApp.Configuration;
using ChatApp.Dtos;
using ChatApp.Storage.Entities;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ChatApp.Storage
{
    public class CosmosConversationStore : IConversationStore
    {
        private readonly CosmosClient _cosmosClient;

        public CosmosConversationStore(IOptions<CosmosSettings> options)
        {
            _cosmosClient = new CosmosClient(options.Value.ConnectionString);
        }

        private Container Container => _cosmosClient.GetDatabase("chatapp").GetContainer("conversation");
        private static ConversationEntity ToEntity(Conversation conversation)
        {
            return new ConversationEntity(
                partitionKey: conversation.username,
                id: conversation.conversationId,
                conversation.participant,
                conversation.lastModifiedUnixTime);
        }
        private static Conversation ToConversation(ConversationEntity entity)
        {
            return new Conversation(
                username: entity.partitionKey,
                conversationId: entity.id,
                participant: entity.participant,
                lastModifiedUnixTime: entity.lastModifiedUnixTime);
        }


        public async Task AddConversation(Conversation conversation)
        {
            if (conversation == null || string.IsNullOrWhiteSpace(conversation.conversationId) || string.IsNullOrWhiteSpace(conversation.username)
                || string.IsNullOrWhiteSpace(conversation.participant) || string.IsNullOrWhiteSpace(conversation.lastModifiedUnixTime.ToString()))
            {
                throw new ArgumentException($"Invalid conversation{conversation}", nameof(conversation));
            }
            await Container.CreateItemAsync(ToEntity(conversation));
        }

        public async Task UpdateConversation(Conversation conversation)
        {
            if (conversation == null || string.IsNullOrWhiteSpace(conversation.conversationId) || string.IsNullOrWhiteSpace(conversation.username)
                || string.IsNullOrWhiteSpace(conversation.participant) || string.IsNullOrWhiteSpace(conversation.lastModifiedUnixTime.ToString()))
            {
                throw new ArgumentException($"Invalid conversation{conversation}", nameof(conversation));
            }

            var existingconversation = await Container.ReadItemAsync<Conversation>(conversation.conversationId, new PartitionKey(conversation.username));
            if (existingconversation == null) {
                throw new ArgumentException($"conversation{conversation} doesn't exist", nameof(conversation));
            }
            else
            {
                await Container.UpsertItemAsync(ToEntity(conversation));
            }
        }

        public async Task<(List<ListConversationsResponseItemSchema>,string)> ListConversations(string username, int limit, long lastSeenConversationTime,string continuationToken)
        {
            List<ListConversationsResponseItemSchema> conversationsSchema = new List<ListConversationsResponseItemSchema>();
            var queryOptions = new QueryRequestOptions() { MaxItemCount = limit, PartitionKey = new PartitionKey(username) };

            QueryDefinition queryDefinition = new QueryDefinition("SELECT m.id, m.participant, m.lastModifiedUnixTime " +
                "FROM m WHERE m.partitionKey=@username " +
                "AND m.lastModifiedUnixTime >= @lastSeenConversationTime " +
                "ORDER BY m.lastModifiedUnixTime DESC ")
                .WithParameter("@username", username)
                .WithParameter("@lastSeenConversationTime", lastSeenConversationTime);

            FeedIterator<ListConversationsResponseItemSchema> feedIterator = Container.GetItemQueryIterator<ListConversationsResponseItemSchema>(queryDefinition,
                continuationToken: continuationToken,
                requestOptions: queryOptions);

            var responseToken = "";
            while (feedIterator.HasMoreResults && conversationsSchema.Count < limit)
            {
                FeedResponse<ListConversationsResponseItemSchema> response = await feedIterator.ReadNextAsync();
                conversationsSchema.AddRange(response.ToList());
                responseToken = response.ContinuationToken;
            }
            return (conversationsSchema, responseToken);
        }
    }
}
