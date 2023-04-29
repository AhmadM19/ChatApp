using ChatApp.Storage;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Dtos;
using ChatApp.Services;
using ChatApp.Exceptions;

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
            try
            {
                var profile = await _profileService.GetProfile(username);
                return Ok(profile);
            }
            catch(ProfileNotFoundException e)
            {
                return NotFound(e.Message);
            }      
        }

        [HttpPost]
        public async Task<ActionResult<Profile>> CreateProfile(Profile profile)
        {
            try
            {
                await _profileService.CreateProfile(profile);
                return CreatedAtAction(nameof(GetProfile), new { username = profile.username },  profile);
            }
            catch(ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch(DuplicateProfileException e)
            {
                return Conflict(e.Message);
            }
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteProfile(string username)
        {
            try
            {
                await _profileService.DeleteProfile(username);
                return Ok();
            }
            catch (ProfileNotFoundException e)
            {
                return NotFound(e.Message);
            }
        }
    }
}