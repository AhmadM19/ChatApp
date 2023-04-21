using ChatApp.Dtos;
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

        public Task DeleteProfile(string username)
        {
            return _profileStore.DeleteProfile(username);
        }

        public Task<Profile?> GetProfile(string username)
        {
            return _profileStore.GetProfile(username);
        }

        public Task UpsertProfile(Profile profile)
        {
            return _profileStore.UpsertProfile(profile);
        }
    }
}
