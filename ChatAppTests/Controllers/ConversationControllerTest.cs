using Azure.Core;
using ChatApp.Dtos;
using ChatApp.Exceptions;
using ChatApp.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatAppTests.Controllers
{
    public class ConversationControllerTests: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly Mock<IConversationService> _conversationServiceMock = new();
        private readonly HttpClient _httpClient;

        public ConversationControllerTests(WebApplicationFactory<Program> factory)
        {
            _httpClient = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => { services.AddSingleton(_conversationServiceMock.Object); });
            }).CreateClient();
        }

        [Fact]
        public async Task SendMessage()
        {
            SendMessageRequest request = new SendMessageRequest("123", "Hello!", "foo");
            var createdUnixTime = 10000;
            _conversationServiceMock.Setup(m => m.SendMessage("foo_bar", "123", "foo", "Hello!")).ReturnsAsync(createdUnixTime);

            var response = await _httpClient.PostAsync("api/conversations/foo_bar/messages", 
                new StringContent(JsonConvert.SerializeObject(request), Encoding.Default, "application/json"));
            var returnedJson = await response.Content.ReadAsStringAsync();
            var responseMessage = JsonConvert.DeserializeObject<SendMessageResponse>(returnedJson);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(createdUnixTime, responseMessage.createdUnixTime);
        }

        [Fact]
        public async Task SendMessage_Duplicate()
        {
            SendMessageRequest request = new SendMessageRequest("123", "Hello!", "foo");
            _conversationServiceMock.Setup(m => m.SendMessage("foo_bar", "123", "foo", "Hello!")).
                ThrowsAsync(new DuplicateMessageException("123"));

            var response = await _httpClient.PostAsync("api/conversations/foo_bar/messages",
            new StringContent(JsonConvert.SerializeObject(request), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Theory]
        [InlineData("","Hello","foo")]
        [InlineData(" ", "Hello", "foo")]
        [InlineData(null, "Hello", "foo")]
        [InlineData("123", "", "foo")]
        [InlineData("123", " ", "foo")]
        [InlineData("123", null, "foo")]
        [InlineData("123", "Hello", "")]
        [InlineData("123", "Hello", " ")]
        [InlineData("123", "Hello", null)]
        public async Task SendMessage_InvalidArgs(string id,string text,string senderUsername)
        {
            SendMessageRequest request = new SendMessageRequest(id,text,senderUsername);

            var response = await _httpClient.PostAsync("api/conversations/foo_bar/messages",
            new StringContent(JsonConvert.SerializeObject(request), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendMessage_ConversationNotFound()
        {
            SendMessageRequest request = new SendMessageRequest("123", "Hello!", "foo");
            _conversationServiceMock.Setup(m => m.SendMessage("foo_bar", "123", "foo", "Hello!")).
                ThrowsAsync(new ConversationNotFoundException("foo_bar"));

            var response = await _httpClient.PostAsync("api/conversations/foo_bar/messages",
            new StringContent(JsonConvert.SerializeObject(request), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task SendMessage_ProfileNotFound()
        {
            SendMessageRequest request = new SendMessageRequest("123", "Hello!", "foo");
            _conversationServiceMock.Setup(m => m.SendMessage("foo_bar", "123", "foo", "Hello!")).
                ThrowsAsync(new ProfileNotFoundException(""));

            var response = await _httpClient.PostAsync("api/conversations/foo_bar/messages",
            new StringContent(JsonConvert.SerializeObject(request), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task SendMessage_StorageUnavailable()
        {
            SendMessageRequest request = new SendMessageRequest("123", "Hello!", "foo");
            _conversationServiceMock.Setup(m => m.SendMessage("foo_bar", "123", "foo", "Hello!")).
                ThrowsAsync(new StorageUnavailableException(""));

            var response = await _httpClient.PostAsync("api/conversations/foo_bar/messages",
            new StringContent(JsonConvert.SerializeObject(request), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        [Fact]
        public async Task AddConversation()
        {
            SendMessageRequest messageRequest = new SendMessageRequest("123", "Hello!", "foo");
            string[] participants = { "foo", "bar" };
            AddConversationRequest conversationRequest = new AddConversationRequest(messageRequest,participants);
            _conversationServiceMock.Setup(m => m.AddConversation(messageRequest, participants)).ReturnsAsync(("foo_bar",1000));

            var response = await _httpClient.PostAsync("api/conversations", 
                new StringContent(JsonConvert.SerializeObject(conversationRequest), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task AddConversation_DuplicateConversation()
        {
            SendMessageRequest messageRequest = new SendMessageRequest("123", "Hello!", "foo");
            string[] participants = { "foo", "bar" };
            string[] result = { "foo_bar", "1000" };
            AddConversationRequest conversationRequest = new AddConversationRequest(messageRequest, participants);
            _conversationServiceMock.Setup(m => m.AddConversation(messageRequest, participants)).
                ThrowsAsync(new DuplicateConversationException(""));

            var response = await _httpClient.PostAsync("api/conversations",
                new StringContent(JsonConvert.SerializeObject(conversationRequest), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task AddConversation_DuplicateMessage()
        {
            SendMessageRequest messageRequest = new SendMessageRequest("123", "Hello!", "foo");
            string[] participants = { "foo", "bar" };
            string[] result = { "foo_bar", "1000" };
            AddConversationRequest conversationRequest = new AddConversationRequest(messageRequest, participants);
            _conversationServiceMock.Setup(m => m.AddConversation(messageRequest, participants)).
                ThrowsAsync(new DuplicateMessageException(""));

            var response = await _httpClient.PostAsync("api/conversations",
                new StringContent(JsonConvert.SerializeObject(conversationRequest), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Theory]
        [InlineData("", "Hello", "foo")]
        [InlineData(" ", "Hello", "foo")]
        [InlineData(null, "Hello", "foo")]
        [InlineData("123", "", "foo")]
        [InlineData("123", " ", "foo")]
        [InlineData("123", null, "foo")]
        [InlineData("123", "Hello", "")]
        [InlineData("123", "Hello", " ")]
        [InlineData("123", "Hello", null)]
        public async Task AddConversation_InvalidArgs(string id, string text,string senderUsername)
        {
            SendMessageRequest messageRequest = new SendMessageRequest(id, text, senderUsername);
            string[] participants = { senderUsername, "bar" };
            AddConversationRequest conversationRequest = new AddConversationRequest(messageRequest, participants);

            var response = await _httpClient.PostAsync("api/conversations",
                new StringContent(JsonConvert.SerializeObject(conversationRequest), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddConversation_ProfileNotFound()
        {
            SendMessageRequest messageRequest = new SendMessageRequest("123", "Hello!", "foo");
            string[] participants = { "foo", "bar" };
            string[] result = { "foo_bar", "1000" };
            AddConversationRequest conversationRequest = new AddConversationRequest(messageRequest, participants);
            _conversationServiceMock.Setup(m => m.AddConversation(messageRequest, participants)).
                ThrowsAsync(new ProfileNotFoundException(""));

            var response = await _httpClient.PostAsync("api/conversations",
                new StringContent(JsonConvert.SerializeObject(conversationRequest), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddConversation_StorageUnavailable()
        {
            SendMessageRequest messageRequest = new SendMessageRequest("123", "Hello!", "foo");
            string[] participants = { "foo", "bar" };
            string[] result = { "foo_bar", "1000" };
            AddConversationRequest conversationRequest = new AddConversationRequest(messageRequest, participants);
            _conversationServiceMock.Setup(m => m.AddConversation(messageRequest, participants)).
                ThrowsAsync(new StorageUnavailableException(""));

            var response = await _httpClient.PostAsync("api/conversations",
                new StringContent(JsonConvert.SerializeObject(conversationRequest), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        [Fact]
        public async Task ListMessages_NoToken()
        {
            string conversationId = "foo_bar";
            int limit = 10;
            long lastSeenMessageTime = 0;
            string continuationToken = null;
            var messages = new List<ListMessagesResponseItem>
            {
                new ListMessagesResponseItem("Hello!", "foo", 1000),
                new ListMessagesResponseItem("Hi there!", "bar", 1500)
            };
            _conversationServiceMock.Setup(m => m.ListMessages(conversationId, limit, lastSeenMessageTime, continuationToken)).ReturnsAsync((messages, continuationToken));

            var response = await _httpClient.GetAsync($"api/conversations/{conversationId}/messages");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(messages, JsonConvert.DeserializeObject<ListMessagesResponse>(json).messages);
            _conversationServiceMock.Verify(m => m.ListMessages(conversationId, limit, lastSeenMessageTime, continuationToken), Times.Once);
        }

        [Fact]
        public async Task ListMessages_TokenReturned()
        {
            string conversationId = "foo_bar";
            int limit = 10;
            long lastSeenMessageTime = 0;
            string continuationToken = null;
            var messages = new List<ListMessagesResponseItem>
            {
                new ListMessagesResponseItem("Hello!", "foo", 1000),
                new ListMessagesResponseItem("Hi there!", "bar", 1500)
            };
            string nextContinuationToken = "ABC123";
            var nextUri = $"/api/conversations/{conversationId}/messages?limit={limit}&lastSeenMessageTime={lastSeenMessageTime}&continuationToken={nextContinuationToken}";
            _conversationServiceMock.Setup(m => m.ListMessages(conversationId, limit, lastSeenMessageTime, continuationToken)).ReturnsAsync((messages, nextContinuationToken));

            var response = await _httpClient.GetAsync($"api/conversations/{conversationId}/messages?limit={limit}&lastSeenMessageTime={lastSeenMessageTime}&continuationToken={null}");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(messages, JsonConvert.DeserializeObject<ListMessagesResponse>(json).messages);
            Assert.Equal(nextUri, JsonConvert.DeserializeObject<ListMessagesResponse>(json).nextUri);
            _conversationServiceMock.Verify(m => m.ListMessages(conversationId, limit, lastSeenMessageTime, continuationToken), Times.Once);
        }

        [Fact]
        public async Task ListMessages_InvalidArgs()
        {
            var response =await _httpClient.GetAsync($"api/conversations/ /messages");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ListMessages_ConversationNotFound()
        {
            string conversationId = "foo_bar";
            int limit = 10;
            long lastSeenMessageTime = 0;
            string continuationToken = null;
            var messages = new List<ListMessagesResponseItem>
            {
                new ListMessagesResponseItem("Hello!", "foo", 1000),
                new ListMessagesResponseItem("Hi there!", "bar", 1500)
            };
            string nextContinuationToken = "ABC123";
            var nextUri = $"/api/conversations/{conversationId}/messages?limit={limit}&lastSeenMessageTime={lastSeenMessageTime}&continuationToken={nextContinuationToken}";
            _conversationServiceMock.Setup(m => m.ListMessages(conversationId, limit, lastSeenMessageTime, continuationToken)).ThrowsAsync(new ConversationNotFoundException(""));

            var response = await _httpClient.GetAsync($"api/conversations/{conversationId}/messages");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ListMessages_MessageNotFound()
        {
            string conversationId = "foo_bar";
            int limit = 10;
            long lastSeenMessageTime = 0;
            string continuationToken = null;
            var messages = new List<ListMessagesResponseItem>
            {
                new ListMessagesResponseItem("Hello!", "foo", 1000),
                new ListMessagesResponseItem("Hi there!", "bar", 1500)
            };
            string nextContinuationToken = "ABC123";
            var nextUri = $"/api/conversations/{conversationId}/messages?limit={limit}&lastSeenMessageTime={lastSeenMessageTime}&continuationToken={nextContinuationToken}";
            _conversationServiceMock.Setup(m => m.ListMessages(conversationId, limit, lastSeenMessageTime, continuationToken)).ThrowsAsync(new MessageNotFoundException(""));

            var response = await _httpClient.GetAsync($"api/conversations/{conversationId}/messages");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ListConversations_NoToken()
        {
            string username = "foo";
            int limit = 10;
            long lastSeenConversationTime = 0;
            string continuationToken = null;
            string nextContinuationToken = null;
            Profile profile = new Profile("foobar", "foo", "bar");
            var conversations = new List<ListConversationsResponseItem>
            {
                new ListConversationsResponseItem("123", profile, 1000),
                new ListConversationsResponseItem("Hi there!", profile, 1500)
            };
            _conversationServiceMock.Setup(m => m.ListConversations(username, limit, lastSeenConversationTime, continuationToken)).ReturnsAsync((conversations,nextContinuationToken));

            var response = await _httpClient.GetAsync($"api/conversations?username={username}&limit={limit}&lastSeenMessageTime={lastSeenConversationTime}&continuationToken={null}");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(conversations, JsonConvert.DeserializeObject<ListConversationsResponse>(json).conversations);
            _conversationServiceMock.Verify(m => m.ListConversations(username, limit, lastSeenConversationTime, continuationToken), Times.Once);
        }

        [Fact]
        public async Task ListConversation_TokenReturned()
        {
            string username = "foo";
            int limit = 10;
            long lastSeenConversationTime = 0;
            string continuationToken = null;
            Profile profile = new Profile("foobar", "foo", "bar");
            var conversations = new List<ListConversationsResponseItem>
            {
                new ListConversationsResponseItem("123", profile, 1000),
                new ListConversationsResponseItem("Hi there!", profile, 1500)
            };
            string nextContinuationToken = "ABC123";
            var nextUri = $"/api/conversations?username={username}&limit={limit}&lastSeenMessageTime={lastSeenConversationTime}&continuationToken={nextContinuationToken}";
            _conversationServiceMock.Setup(m => m.ListConversations(username, limit, lastSeenConversationTime, continuationToken)).ReturnsAsync((conversations, nextContinuationToken));

            var response = await _httpClient.GetAsync($"api/conversations?username={username}&limit={limit}&lastSeenMessageTime={lastSeenConversationTime}&continuationToken={null}");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(conversations, JsonConvert.DeserializeObject<ListConversationsResponse>(json).conversations);
            Assert.Equal(nextUri, JsonConvert.DeserializeObject<ListConversationsResponse>(json).nextUri);
            _conversationServiceMock.Verify(m => m.ListConversations(username, limit, lastSeenConversationTime, continuationToken), Times.Once);
        }

        [Fact]
        public async Task ListConversation_InvalidArgs()
        {
            var response = await _httpClient.GetAsync($"api/conversations?username= ");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ListConversation_ConversationNotFound()
        {
            string username = "foo";
            int limit = 10;
            long lastSeenConversationTime = 0;
            string continuationToken = null;
            _conversationServiceMock.Setup(m => m.ListConversations(username, limit, lastSeenConversationTime, continuationToken)).ThrowsAsync(new ConversationNotFoundException(""));

            var response = await _httpClient.GetAsync($"api/conversations?username={username}&limit={limit}&lastSeenMessageTime={lastSeenConversationTime}&continuationToken={null}");
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            _conversationServiceMock.Verify(m => m.ListConversations(username, limit, lastSeenConversationTime, continuationToken), Times.Once);
        }
    }
}
