using Azure.Storage.Blobs.Models;
using ChatApp.Dtos;
using ChatApp.Exceptions;
using ChatApp.Services;
using ChatApp.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.IntegrationTests
{
    public class ConversationServiceTest
    {
        private readonly Mock<IConversationStore> _conversationStoreMock = new();
        private readonly Mock<IMessageStore> _messageStoreMock = new();
        private readonly Mock<IProfileService> _profileServiceMock = new();
        private readonly Services.ConversationService _conversationService;
        public ConversationServiceTest()
        {
            _conversationService = new Services.ConversationService(_conversationStoreMock.Object, _messageStoreMock.Object, _profileServiceMock.Object);
        }

        [Fact]
        public async Task SendMessage()
        {
            var responseTime = await _conversationService.SendMessage("foo_bar", "123", "foo", "Hello!");
            Assert.IsType<long>(responseTime);

            //Verifying that the time returned is the time used in invoking the store methods
            var message = new Message("foo_bar", "123", "foo", "Hello!", responseTime);
            var userConversation1 = new Conversation("foo", "bar", "foo_bar", responseTime);
            var userConversation2 = new Conversation("bar", "foo", "foo_bar", responseTime);
            _messageStoreMock.Verify(m => m.SendMessage(message), Times.Once);
            _conversationStoreMock.Verify(m => m.UpdateConversation(userConversation1), Times.Once);
            _conversationStoreMock.Verify(m => m.UpdateConversation(userConversation2), Times.Once);
        }

        [Theory]
        [InlineData("foo","123","foo","Hello")]
        [InlineData("", "123", "foo", "Hello")]
        [InlineData(" ", "123", "foo", "Hello")]
        [InlineData(null, "123", "foo", "Hello")]
        [InlineData("foo", "", "foo", "Hello")]
        [InlineData("foo", " ", "foo", "Hello")]
        [InlineData("foo", null, "foo", "Hello")]
        [InlineData("foo", "123", "", "Hello")]
        [InlineData("foo", "123", " ", "Hello")]
        [InlineData("foo", "123", null, "Hello")]
        [InlineData("foo", "123", "foo", "")]
        [InlineData("foo", "123", "foo", " ")]
        [InlineData("foo", "123", "foo", null)]
        public async Task SendMessage_Invalid(string conversationId,string messageId,string sendeUsername,string text)
        {
            Assert.ThrowsAsync<ArgumentException>(() => _conversationService.SendMessage(conversationId, messageId,sendeUsername, text));
        }

        [Fact]
        public async Task AddConversations()
        {
            SendMessageRequest messageRequest = new SendMessageRequest("123", "Hello!", "foo");
            string[] participants = { "foo", "bar" };
            var (conversationId,createdUnixTime) =  await _conversationService.AddConversation(messageRequest, participants);

            //Verifying that the time returned is the time used in invoking the store methods
            var message = new Message("foo_bar", "123", "foo", "Hello!", createdUnixTime);
            var userConversation1 = new Conversation("foo", "bar", "foo_bar", createdUnixTime);
            var userConversation2 = new Conversation("bar", "foo", "foo_bar", createdUnixTime);
            _messageStoreMock.Verify(m => m.SendMessage(message), Times.Once);
            _conversationStoreMock.Verify(m => m.AddConversation(userConversation1), Times.Once);
            _conversationStoreMock.Verify(m => m.AddConversation(userConversation2), Times.Once);
        }

        [Fact]
        public async Task ListMessages()
        {
            var conversationId = "conversation1";
            var limit = 10;
            var lastSeenMessageTime = 0;
            var continuationToken = "";
            var messages = new List<ListMessagesResponseItem>
            {  new ListMessagesResponseItem ( "Hello", "foo",  1000 ), new ListMessagesResponseItem ( "Hi", "bar",  2000 ) };
            var tokenArray = "token";
            _messageStoreMock.Setup(m => m.ListMessages(conversationId, limit, lastSeenMessageTime, continuationToken))
                .ReturnsAsync((messages, tokenArray));

            var (resultMessages, resultToken) = await _conversationService.ListMessages(conversationId, limit, lastSeenMessageTime, continuationToken);
            Assert.Equal(messages, resultMessages);
            Assert.Equal(tokenArray, resultToken);
        }

        [Theory]
        [InlineData("",10,0)]
        [InlineData(" ",10,0)]
        [InlineData(null,10,0)]
        public async Task ListMessages_Invalid(string conversationId,int limit,long lastSeenMessageTime)
        {
            Assert.ThrowsAsync<ArgumentException>(() => _conversationService.ListMessages(conversationId,10,0,null));
        }

        [Fact]
        public async Task ListMessages_MessagesNotFound()
        {
            var conversationId = "conversation1";
            var limit = 10;
            var lastSeenMessageTime = 0;
            var continuationToken = "";
            var messages = new List<ListMessagesResponseItem>();
            var tokenArray = "token";
            _messageStoreMock.Setup(m => m.ListMessages(conversationId, limit, lastSeenMessageTime, continuationToken))
                .ReturnsAsync((messages, tokenArray));

            Assert.ThrowsAsync<MessageNotFoundException>(()=>  _conversationService.ListMessages(conversationId, limit, lastSeenMessageTime, continuationToken));
        }

        [Fact]
        public async Task ListConversations()
        {
            var username = "mike";
            var limit = 10;
            var lastSeenConversationTime = 0;
            var continuationToken = "";
            var tokenArray = "token";
            var conversationSchema = new List<ListConversationsResponseItemSchema> {
                new ListConversationsResponseItemSchema("mike_john","johnSm",1000),new ListConversationsResponseItemSchema("mike_sam","samJa",2000)};
            _conversationStoreMock.Setup(m => m.ListConversations(username, limit, lastSeenConversationTime, continuationToken))
                .ReturnsAsync((conversationSchema, tokenArray));
            var profile1 = new Profile("johnSm", "john", "smith");
            var profile2 = new Profile("samJa", "sam", "jack");
            _profileServiceMock.Setup(x => x.GetProfile("johnSm")).ReturnsAsync(profile1);
            _profileServiceMock.Setup(x => x.GetProfile("samJa")).ReturnsAsync(profile2);
            var conversations = new List<ListConversationsResponseItem>
            { new ListConversationsResponseItem("mike_john",profile1,1000),
              new ListConversationsResponseItem("mike_sam",profile2,2000)};

            var (responseConversations,responseToken) = await _conversationService.ListConversations("mike", 10, 0, "");
            Assert.Equal(conversations, responseConversations);
            Assert.Equal(tokenArray, responseToken);
        }

        [Fact]
        public async Task ListConversations_NotFound()
        {
            var username = "mike";
            var limit = 10;
            var lastSeenConversationTime = 0;
            var continuationToken = "";
            var tokenArray = "token";
            var conversationSchema = new List<ListConversationsResponseItemSchema>();
            _conversationStoreMock.Setup(m => m.ListConversations(username, limit, lastSeenConversationTime, continuationToken))
                .ReturnsAsync((conversationSchema, tokenArray));

            Assert.ThrowsAsync<ConversationNotFoundException>(() => _conversationService.ListConversations(username, limit, lastSeenConversationTime, continuationToken));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task ListConversation_Invalid(string username)
        {
            Assert.ThrowsAsync<ArgumentException>(() => _conversationService.ListConversations(username, 10, 0, null));
        }
    }
}
