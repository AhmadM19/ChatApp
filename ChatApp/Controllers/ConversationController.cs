using ChatApp.Dtos;
using ChatApp.Exceptions;
using ChatApp.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

using System.Web;

namespace ChatApp.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class conversationsController : ControllerBase
    {
        private readonly IConversationService _conversationService;
        private readonly ILogger<conversationsController> _logger;
        TelemetryClient _telemetryClient;


        public conversationsController(IConversationService conversationService, ILogger<conversationsController> logger, TelemetryClient telemetryClient)
        {
            _conversationService = conversationService;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        [HttpPost("{conversationId}/messages")]
        public async Task<ActionResult<SendMessageResponse>> SendMessage(string conversationId,SendMessageRequest request)
        {
            using (_logger.BeginScope("{MessageId}", request.id))
            {
                try
                {
                    _logger.LogInformation("Sending message with id {MessageId}");
                    var createdUnixTime = await _conversationService.SendMessage(conversationId, request.id, request.senderUsername, request.text);
                    _telemetryClient.TrackEvent("MessagesSent");
                    return CreatedAtAction(nameof(ListMessages), new { conversationId = request.id }, new SendMessageResponse(createdUnixTime));
                }
                catch (DuplicateMessageException e)
                {
                    _logger.LogWarning("Message with id {MessageId} is already found");
                    return Conflict(e.Message);
                }
                catch (ArgumentException e)
                {
                    _logger.LogWarning(e.Message);
                    return BadRequest(e.Message);
                }
                catch (ConversationNotFoundException e)
                {
                    _logger.LogWarning($"Conversation with id {conversationId} doesn't exist");
                    return NotFound(e.Message);
                }
                catch (ProfileNotFoundException e)
                {
                    _logger.LogWarning($"Profile with username {request.senderUsername} doesn't exist");
                    return NotFound(e.Message);
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult<AddConversationResponse>> AddConversation(AddConversationRequest request)
        {
            try
            {
                var (conversationId,createdUnixTime) = await _conversationService.AddConversation(request.firstMessage, request.participants);
                using (_logger.BeginScope("{ConversationId}", conversationId))
                {
                    _logger.LogInformation("Starting conversation with Id {ConversationId}");
                    _telemetryClient.TrackEvent("ConversationsStarted");
                }
                return CreatedAtAction(nameof(ListConversations), null, new AddConversationResponse(conversationId, request.participants, new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(createdUnixTime)));
            }
            catch(DuplicateConversationException e)
            {
                _logger.LogWarning(e.Message);
                return Conflict(e.Message);
            }
            catch(DuplicateMessageException e)
            {
                _logger.LogWarning(e.Message);
                return Conflict(e.Message);
            }
            catch(ArgumentException e)
            {
                _logger.LogWarning(e.Message);
                return BadRequest(e.Message);   
            }
            catch(ProfileNotFoundException e)
            {
                _logger.LogWarning(e.Message);
                return NotFound(e.Message); 
            }
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<ActionResult<ListMessagesResponse>> ListMessages(string conversationId, int limit = 10, long lastSeenMessageTime = 0, string? continuationToken = null)
        {
            using (_logger.BeginScope("{ConversationId}", conversationId))
            {
                try
                {
                    _logger.LogInformation("Getting messages of conversation with Id {ConversationId}");
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
                catch (ArgumentException e)
                {   //If no conversationId was found in URL
                    _logger.LogWarning(e.Message);
                    return BadRequest(e.Message);
                }
                catch (ConversationNotFoundException e)
                {   //If conversation doesn't exist
                    _logger.LogWarning("Conversation with Id {ConversationId} doesn't exist");
                    return NotFound(e.Message);
                }
                catch (MessageNotFoundException e)
                {  //If no messages in conversation
                    _logger.LogWarning(e.Message);
                    return NotFound(e.Message);
                }
            }
        }

        [HttpGet]
        public async Task<ActionResult<ListConversationsResponse>> ListConversations(string username, int limit = 10, long lastSeenConversationTime = 0, string? continuationToken = null)
        {
            using (_logger.BeginScope("{Username}", username))
            {
                try
                {
                    _logger.LogInformation("Getting conversations of user with usernmae{Username}");
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
                catch (ArgumentException e)
                {
                    //If no username was found in URL
                    _logger.LogWarning(e.Message);
                    return BadRequest(e.Message);
                }
                catch (ConversationNotFoundException e)
                {   //If no conversations found for the user
                    _logger.LogWarning(e.Message);
                    return NotFound(e.Message);
                }
            }
        }
    }
}
