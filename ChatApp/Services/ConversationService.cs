using ChatApp.Dtos;
using ChatApp.Storage;
using ChatApp.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace ChatApp.Services
{
    public class ConversationService:IConversationService
    {
        private readonly IConversationStore _conversationStore;
        private readonly IMessageStore _messageStore;
        private readonly IProfileService _profileService;

        public ConversationService(IConversationStore conversationStore, IMessageStore messageStore,IProfileService profileService)
        {
            _conversationStore = conversationStore;
            _messageStore = messageStore;
            _profileService = profileService;
        }


        public async Task<long> SendMessage(string conversationId, string messageId, string senderUsername, string text)
        {
            
            var CreatedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var message= new Message(conversationId, messageId, senderUsername, text,CreatedUnixTime);
            await _messageStore.SendMessage(message);

            if (conversationId.Contains('_') || string.IsNullOrWhiteSpace(conversationId))
            {
                string[] participants = conversationId.Split('_');
                //Will throw an exception if profiles don't exist
                await _profileService.GetProfile(participants[0]);
                await _profileService.GetProfile(participants[1]);
                var userConversation1 = new Conversation(participants[0], participants[1], conversationId, CreatedUnixTime);
                var userConversation2 = new Conversation(participants[1], participants[0], conversationId, CreatedUnixTime);
                await _conversationStore.UpdateConversation(userConversation1);
                await _conversationStore.UpdateConversation(userConversation2);
            }
            else { throw new ArgumentException("conversationId is not valid"); }

            return CreatedUnixTime;
        }

        public async Task<string[]> AddConversation(SendMessageRequest firstMessage, params string[] participants)
        {
            var conversationId = participants[0] + "_" + participants[1];
            //Will throw an exception if profiles don't exist
            await _profileService.GetProfile(participants[0]);
            await _profileService.GetProfile(participants[1]);
            var CreatedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var message = new Message(conversationId, firstMessage.id, firstMessage.senderUsername, firstMessage.text, CreatedUnixTime);
            await _messageStore.SendMessage(message);

            var userConversation1=new Conversation(participants[0], participants[1],conversationId,CreatedUnixTime);
            var userConversation2 = new Conversation(participants[1], participants[0], conversationId, CreatedUnixTime);
            await _conversationStore.AddConversation(userConversation1);
            await _conversationStore.AddConversation(userConversation2);

            return new string[] { conversationId, CreatedUnixTime.ToString() };
        }

        public async Task<(List<ListMessagesResponseItem>,string)> ListMessages(string conversationId, int limit, long lastSeenMessageTime,string continuationToken)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                throw new ArgumentException("ConversationId must be provided");
            }
            var messages=new List<ListMessagesResponseItem>();
            string tokenArray="";
            (messages,tokenArray) = await _messageStore.ListMessages(conversationId, limit, lastSeenMessageTime,continuationToken);
            if (messages.Count == 0) 
            {
                throw new MessageNotFoundException(conversationId);
            }
            return (messages,tokenArray);
        }

        public async Task<(List<ListConversationsResponseItem>, string)> ListConversations(string username, int limit, long lastSeenConservationTime, string continuationToken)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username must be provided");
            }
            var (conversationsSchema, tokenArray) = await _conversationStore.ListConversations(username, limit, lastSeenConservationTime, continuationToken);
            if (conversationsSchema.Count == 0)
            {
                throw new ConversationNotFoundException("");
            }
            var conversations=new List<ListConversationsResponseItem>();
            foreach(ListConversationsResponseItemSchema list in conversationsSchema)
            {
                var profile = await _profileService.GetProfile(list.participant);
                conversations.Add(new ListConversationsResponseItem(list.id, profile, list.lastModifiedUnixTime));
            }
            return (conversations,tokenArray);
        }
    }

}
