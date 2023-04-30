using ChatApp.Dtos;
using ChatApp.Exceptions;
using ChatApp.Storage;
using ChatApp.Storage.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.IntegrationTests
{
    public class ConversationStoreTests: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly IConversationStore _conversationStore;

        public ConversationStoreTests(WebApplicationFactory<Program> factory)
        {
            _conversationStore = factory.Services.GetRequiredService<IConversationStore>();
        }

        [Fact]
        public async Task AddConversation()
        {
            var conversation = new Conversation("koo", "hoo", "koo_hoo", 20000);
            await _conversationStore.AddConversation(conversation);
            Assert.Equal(conversation, await _conversationStore.GetConversation(conversation.username, conversation.conversationId));

            await _conversationStore.DeleteConversation(conversation.username, conversation.conversationId);
        }

        [Theory]
        [InlineData("", "bar", "foobar")]
        [InlineData(" ", "bar", "foobar")]
        [InlineData(null, "bar", "foobar")]
        [InlineData("foo", "", "foobar")]
        [InlineData("foo", " ", "foobar")]
        [InlineData("foo",null , "foobar")]
        [InlineData("foo", "bar", "")]
        [InlineData("", "bar", " ")]
        [InlineData("", "bar", null)]
        public async Task AddConversation_Invalid(string username,string participant,string conversationId)
        {
            var conversation = new Conversation(username,participant,conversationId, 1000);
            Assert.ThrowsAsync<ArgumentException>(() =>  _conversationStore.AddConversation(conversation));
        }

        [Fact]
        public async Task AddConversation_Duplicate()
        {
            var conversation = new Conversation("joo","boo", "joo_boo", 1000);
            await _conversationStore.AddConversation(conversation);
            Assert.ThrowsAsync<DuplicateConversationException>(() => _conversationStore.AddConversation(conversation));    

            await _conversationStore.DeleteConversation(conversation.username,conversation.conversationId);
        }

        [Fact]
        public async Task GetConversation()
        {
            var conversation = new Conversation("voo", "boo", "voo_boo", 1000);
            await _conversationStore.AddConversation(conversation);
            var responseConversation = await _conversationStore.GetConversation(conversation.username,conversation.conversationId);
            Assert.Equal(conversation.username, responseConversation.username);
            Assert.Equal(conversation.participant, responseConversation.participant);
            Assert.Equal(conversation.conversationId, responseConversation.conversationId);
            Assert.Equal(conversation.lastModifiedUnixTime, responseConversation.lastModifiedUnixTime);

            await _conversationStore.DeleteConversation(conversation.username, conversation.conversationId);
        }

        [Fact]
        public async Task UpdateConversation()
        {
            var conversation = new Conversation("coo", "hoo", "coo_hoo", 20000);
            await _conversationStore.AddConversation(conversation);
            var updatedConversation = new Conversation("coo", "hoo", "coo_hoo", 21000);
            await _conversationStore.UpdateConversation(updatedConversation);
            Assert.Equal(updatedConversation, await _conversationStore.GetConversation("coo", "coo_hoo"));

            await _conversationStore.DeleteConversation(conversation.username, conversation.conversationId);
        }

        [Theory]
        [InlineData("", "bar", "foobar")]
        [InlineData(" ", "bar", "foobar")]
        [InlineData(null, "bar", "foobar")]
        [InlineData("foo", "", "foobar")]
        [InlineData("foo", " ", "foobar")]
        [InlineData("foo", null, "foobar")]
        [InlineData("foo", "bar", "")]
        [InlineData("", "bar", " ")]
        [InlineData("", "bar", null)]
        public async Task UpdateConversation_Invalid(string username, string participant, string conversationId)
        {
            var conversation = new Conversation(username, participant, conversationId, 1000);
            Assert.ThrowsAsync<ArgumentException>(() => _conversationStore.UpdateConversation(conversation));
        }

        [Fact]
        public async Task UpdateConversation_NotFound()
        {
            var conversation = new Conversation("zoo", "hoo", "zoo_hoo", 20000);
            Assert.ThrowsAsync<ConversationNotFoundException>(() => _conversationStore.UpdateConversation(conversation));
        }

        [Fact]
        public async Task ListConversations()
        {
            var conversation1 = new Conversation("soo", "qoo", "soo_qoo", 20000);
            var conversation2 = new Conversation("soo", "woo", "soo_woo", 21000);
            await _conversationStore.AddConversation(conversation1);
            await _conversationStore.AddConversation(conversation2);
            //response conversations ordered by time
            var conversationSchema = new List<ListConversationsResponseItemSchema> {
                new ListConversationsResponseItemSchema("soo_woo", "woo", 21000),
                new ListConversationsResponseItemSchema("soo_qoo", "qoo", 20000)};
            var (responseSchema,token) = await _conversationStore.ListConversations("soo", 10, 0, null);
            Assert.Equivalent(conversationSchema,responseSchema);

            await _conversationStore.DeleteConversation("soo","soo_qoo");
            await _conversationStore.DeleteConversation("soo", "soo_woo");
        }
    }
}
