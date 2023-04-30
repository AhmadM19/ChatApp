using ChatApp.Dtos;
using ChatApp.Exceptions;
using ChatApp.Storage;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.IntegrationTests
{
    public class MessageStoreTests: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly IMessageStore _messageStore;

        public MessageStoreTests(WebApplicationFactory<Program> factory)
        {
            _messageStore = factory.Services.GetRequiredService<IMessageStore>();
        }

        [Fact]
        public async Task SendMessage()
        {
            var message = new Message("foo_bar", "126", "foo", "Hey", 10000);
            await _messageStore.SendMessage(message);
            Assert.Equal(message, await _messageStore.GetMessage("foo_bar", "126"));

            await _messageStore.DeleteMessage(message.conversationId,message.messageId);
        }

        [Fact]
        public async Task SendMessage_Duplicate()
        {
            var message = new Message("foo_bar", "129", "foo", "Hey", 10000);
            await _messageStore.SendMessage(message);
            Assert.ThrowsAsync<DuplicateMessageException>(() => _messageStore.SendMessage(message));

            await _messageStore.DeleteMessage(message.conversationId,message.messageId);
        }

        [Theory]
        [InlineData("","123","foo","Hey")]
        [InlineData(" ", "123", "foo", "Hey")]
        [InlineData(null, "123", "foo", "Hey")]
        [InlineData("foo_bar", "", "foo", "Hey")]
        [InlineData("foo_bar", " ", "foo", "Hey")]
        [InlineData("foo_bar", null, "foo", "Hey")]
        [InlineData("foo_bar", "123", "", "Hey")]
        [InlineData("foo_bar", "123", " ", "Hey")]
        [InlineData("foo_bar", "123", null, "Hey")]
        [InlineData("foo_bar", "123", "foo", "")]
        [InlineData("foo_bar", "123", "foo", " ")]
        [InlineData("foo_bar", "123", "foo", null)]
        public async Task SendMessage_InvalidArgs(string conversationId,string messageId,string senderUsername,string text)
        {
            var message=new Message(conversationId, messageId, senderUsername, text,1000);   
            Assert.ThrowsAsync<ArgumentException>(() => _messageStore.SendMessage(message));
        }

        [Fact]
        public async Task GetMessage()
        {
            var message = new Message("foo_bar", "252", "foo","Hey", 1000);
            await _messageStore.SendMessage(message);
            var responseMessage = await _messageStore.GetMessage("foo_bar", "252");
            Assert.Equal(message.conversationId, responseMessage.conversationId);
            Assert.Equal(message.messageId, responseMessage.messageId);
            Assert.Equal(message.senderUsername, responseMessage.senderUsername);
            Assert.Equal(message.text, responseMessage.text);
            Assert.Equal(message.createdUnixTime, responseMessage.createdUnixTime);

            await _messageStore.DeleteMessage(message.conversationId, message.messageId);
        }

        [Fact]
        public async Task ListMessages()
        {
            var message1 = new Message("foo_bar", "173", "foo", "Hey", 1000);
            var message2 = new Message("foo_bar", "631", "bar", "Hi", 2000);
            //response messages ordered by time
            var messages = new List<ListMessagesResponseItem>
            {
                new ListMessagesResponseItem("Hi","bar",2000),
                new ListMessagesResponseItem("Hey","foo",1000)
            };
            await _messageStore.SendMessage(message1);
            await _messageStore.SendMessage(message2);
            var(response, token) = await _messageStore.ListMessages("foo_bar", 10, 0, null);
            Assert.Equivalent(messages, response);

            await _messageStore.DeleteMessage(message1.conversationId, message1.messageId);
            await _messageStore.DeleteMessage(message2.conversationId, message2.messageId);
        }
    }
}
