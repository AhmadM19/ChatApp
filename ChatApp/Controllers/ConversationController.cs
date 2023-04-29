using ChatApp.Dtos;
using ChatApp.Exceptions;
using ChatApp.Services;
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

        [HttpPost("{conversationId}/messages")]
        public async Task<ActionResult<SendMessageResponse>> SendMessage(string conversationId,SendMessageRequest request)
        {
            try
            {
                var createdUnixTime = await _conversationService.SendMessage(conversationId, request.id, request.senderUsername, request.text);
                return CreatedAtAction(nameof(ListMessages), new { conversationId = request.id }, new SendMessageResponse(createdUnixTime));
            }
            catch(DuplicateMessageException e)
            {
                return Conflict(e.Message);
            }
            catch(ArgumentException e)
            {
                return BadRequest(e.Message);  
            }
            catch(ConversationNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch(ProfileNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<AddConversationResponse>> AddConversation(AddConversationRequest request)
        {
            try
            {
                var response = await _conversationService.AddConversation(request.firstMessage, request.participants);
                string conversationId = response[0];
                string createdUnixTime = response[1];
                return CreatedAtAction(nameof(ListConversations), null, new AddConversationResponse(conversationId, request.participants, new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(long.Parse(createdUnixTime))));
            }
            catch(DuplicateConversationException e)
            {
                return Conflict(e.Message);
            }
            catch(DuplicateMessageException e)
            {
                return Conflict(e.Message);
            }
            catch(ArgumentException e)
            {
                return BadRequest(e.Message);   
            }
            catch(ProfileNotFoundException e)
            {
                return NotFound(e.Message); 
            }
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<ActionResult<ListMessagesResponse>> ListMessages(string conversationId, int limit = 10, long lastSeenMessageTime = 0, string? continuationToken = null)
        {
            try
            {
                var (messages, nextContinuationToken) = await _conversationService.ListMessages(conversationId, limit, lastSeenMessageTime, continuationToken);
                if (nextContinuationToken != null)
                {
                    nextContinuationToken = HttpUtility.UrlEncode(nextContinuationToken).Replace("%5C", "");
                }
                var nextUri = nextContinuationToken != null
                    ? $"/api/conversations/{conversationId}/messages?limit={limit}&lastSeenMessageTime={lastSeenMessageTime}&continuationToken={nextContinuationToken}"
                    : null;
                return Ok(new ListMessagesResponse(messages, nextUri));
            }
            catch(ArgumentException e)
            {   //If no conversationId was found in URL
                return BadRequest(e.Message);   
            }
            catch(ConversationNotFoundException e)
            {   //If conversation doesn't exist
                return NotFound(e.Message);
            }
            catch(MessageNotFoundException e)
            {  //If no messages in conversation
                return NotFound(e.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ListConversationsResponse>> ListConversations(string username, int limit = 10, long lastSeenConversationTime = 0, string? continuationToken = null)
        {
            try
            {
                var (conversations, nextContinuationToken) = await _conversationService.ListConversations(username, limit, lastSeenConversationTime, continuationToken);
                if (nextContinuationToken != null)
                {
                    nextContinuationToken = HttpUtility.UrlEncode(nextContinuationToken).Replace("%5C", "");
                }
                var nextUri = nextContinuationToken != null
                    ? $"/api/conversations?username={username}&limit={limit}&lastSeenMessageTime={lastSeenConversationTime}&continuationToken={nextContinuationToken}"
                    : null;
                return Ok(new ListConversationsResponse(conversations, nextUri));
            }
            catch(ArgumentException e)
            {
                //If no username was found in URL
                return BadRequest(e.Message);
            }
            catch(ConversationNotFoundException e)
            {   //If no conversations found for the user
                return NotFound(e.Message);
            }
        }
    }
}
