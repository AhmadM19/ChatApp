using ChatApp.Storage;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Dtos;
using ChatApp.Services;
using ChatApp.Exceptions;
using Microsoft.ApplicationInsights;

namespace ChatApp.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class profileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly ILogger<profileController> _logger;
        TelemetryClient _telemetryClient;
        public profileController(IProfileService profileService, ILogger<profileController> logger, TelemetryClient telemetryClient)
        {
            _profileService = profileService;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }


        [HttpGet("{username}")]
        public async Task<ActionResult<Profile>> GetProfile(string username)
        {
            using (_logger.BeginScope("{Username}", username))
            {
                try
                {
                    _logger.LogInformation("Getting Profile of user {Username}");
                    var profile = await _profileService.GetProfile(username);
                    return Ok(profile);
                }
                catch (ProfileNotFoundException e)
                {
                    _logger.LogWarning("Profile with username {Username} was not found");
                    return NotFound(e.Message);
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult<Profile>> CreateProfile(Profile profile)
        {
            using (_logger.BeginScope("{Username}", profile.username))
            {
                try
                {
                    _logger.LogInformation("Creating Profile for user {Username}");
                    await _profileService.CreateProfile(profile);
                    _telemetryClient.TrackEvent("ProfilesCreated");
                    return CreatedAtAction(nameof(GetProfile), new { username = profile.username }, profile);
                }
                catch (ArgumentException e)
                {
                    _logger.LogWarning(e.Message);
                    return BadRequest(e.Message);
                }
                catch (DuplicateProfileException e)
                {
                    _logger.LogWarning($"Duplicate profile {e.Message}");
                    return Conflict(e.Message);
                }
            }
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteProfile(string username)
        {
            using (_logger.BeginScope("{Username}", username))
            {
                try
                {
                    _logger.LogInformation("Deleting Profile for user {Username}");
                    await _profileService.DeleteProfile(username);
                    return Ok();
                }
                catch (ProfileNotFoundException e)
                {
                    _logger.LogWarning("Profile of user {Username} was not found");
                    return NotFound(e.Message);
                }
            }
        }
    }
}