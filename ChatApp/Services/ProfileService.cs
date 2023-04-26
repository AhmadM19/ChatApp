using ChatApp.Dtos;
using ChatApp.Exceptions;
using ChatApp.Storage;

namespace ChatApp.Services
{
    public class ProfileService: IProfileService
    {
        private readonly IProfileStore _profileStore;

        public ProfileService(IProfileStore profileStore)
        {
            _profileStore = profileStore;
        }

        public async Task<Profile?> GetProfile(string username)
        {
            var profile =  _profileStore.GetProfile(username);
            if(profile.Result == null)
            {
                throw new ProfileNotFoundException(username);
            }
            return await _profileStore.GetProfile(username);
        }

        public async Task CreateProfile(Profile profile)
        {

            var existingProfile =  _profileStore.GetProfile(profile.username);
            if(existingProfile.Result != null) 
            {
                throw new DuplicateProfileException(profile.username);
            }
            await _profileStore.CreateProfile(profile);

        }
        public async Task DeleteProfile(string username)
        {
            var profile = _profileStore.GetProfile(username);
            if (profile.Result == null)
            {
                throw new ProfileNotFoundException(username);
            }
            await _profileStore.DeleteProfile(username);
        }
    }
}
