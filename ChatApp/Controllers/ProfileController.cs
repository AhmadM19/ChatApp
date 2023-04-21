using ChatApp.Storage;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Dtos;
using ChatApp.Services;

namespace ChatApp.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class profileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        public profileController(IProfileService profileService)
        {
            _profileService= profileService;
        }


        [HttpGet("{username}")]
        public async Task<ActionResult<Profile>> GetProfile(string username)
        {
            var profile = await _profileService.GetProfile(username);
            if (profile == null)
            {
                return NotFound($"The user with {username} is not found");
            }
            return Ok(profile);
        }

        [HttpPost]
        public async Task<ActionResult<Profile>> AddProfile(Profile profile)
        {
            var existingProfile = await _profileService.GetProfile(profile.username);
            if (existingProfile != null) 
            {
                return Conflict($"A User with username{profile.username} already exists");
            }

            await _profileService.UpsertProfile(profile);
            return CreatedAtAction(nameof(GetProfile), new { username = profile.username },
                   profile);
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteProfile(string username)
        {
            var profile = await _profileService.GetProfile(username);
            if (profile == null)
            {
                return NotFound($"The user with {username} is not found");
            }
            await _profileService.DeleteProfile(profile.username);
            return Ok();
        }

    }
}