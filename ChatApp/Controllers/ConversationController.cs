using ChatApp.Dtos;
using ChatApp.Services;
using ChatApp.Storage;
using Microsoft.AspNetCore.Mvc;

using System.Web;

namespace ChatApp.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class conversationsController : ControllerBase
    {
        private readonly IConversationService _conversationService;
       
        public conversationsController(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        [HttpPost("{conversationId}/messages",Name ="conversations")]
        public async Task<ActionResult<SendMessageResponse>> SendMessage(string conversationId,SendMessageRequest request)
        {
            var createdUnixTime = await _conversationService.SendMessage(conversationId, request.messageId, request.senderUsername, request.text);
            return CreatedAtAction(nameof(ListMessages), new { id = request.messageId }, new SendMessageResponse(createdUnixTime));
        }

        [HttpPost]
        public async Task<ActionResult<AddConversationResponse>> AddConversation(AddConversationRequest request)
        {
            var response= await _conversationService.AddConversation(request.firstMessage,request.participants);
            string conversationId = response[0];
            string createdUnixTime = response[1];
            //return CreatedAtAction(nameof(), new { id = conversationId }, new AddConversationResponse(conversationId, request.participants, DateTimeOffset.FromUnixTimeSeconds(long.Parse(createdUnixTime)).UtcDateTime));
            return Ok(new AddConversationResponse(conversationId, request.participants, new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(long.Parse(createdUnixTime))));
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<ActionResult<ListMessagesResponse>> ListMessages(string conversationId, int limit = 10, long lastSeenMessageTime = 0, string? continuationToken = null)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                return BadRequest("ConversationId must be provided");
            }
            else
            {
                var (messages, nextContinuationToken) = await _conversationService.ListMessages(conversationId, limit, lastSeenMessageTime, continuationToken);
                if(nextContinuationToken!= null)
                {
                    nextContinuationToken=HttpUtility.UrlEncode(nextContinuationToken).Replace("%5C","");
                }
                var nextUri = nextContinuationToken != null
                    ? $"/api/Conversation/{conversationId}/messages?limit={limit}&lastSeenMessageTime={lastSeenMessageTime}&continuationToken={nextContinuationToken}"
                    : null;
                return Ok(new ListMessagesResponse(messages, nextUri));
            }
        }


        [HttpGet]
        public async Task<ActionResult<ListConversationsResponse>> ListConversations(string username, int limit = 10, long lastSeenConversationTime = 0, string? continuationToken = null)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("Username must be provided");
            }
            else
            {
                var (conversations, nextContinuationToken) = await _conversationService.ListConversations(username, limit, lastSeenConversationTime, continuationToken);
                if (nextContinuationToken != null)
                {
                    nextContinuationToken = HttpUtility.UrlEncode(nextContinuationToken).Replace("%5C", "");
                }
                var nextUri = nextContinuationToken != null
                    ? $"/api/Conversation/messages?username={username}&limit={limit}&lastSeenMessageTime={lastSeenConversationTime}&continuationToken={nextContinuationToken}"
                    : null;
                return Ok(new ListConversationsResponse(conversations, nextUri));
            }
        }

    }
}
