using ChatApp.Dtos;

namespace ChatApp.Services
{
    public interface IProfileService
    {
        /// <exception cref="ProfileNotFoundException">is thrown if the profile does not exists</exception>
        /// <exception cref="StorageUnavailableException">the database is unavailable</exception>
        /// <exception cref="DuplicateProfileException">is thrown if the profile already exists</exception>
        Task UpsertProfile(Profile profile);
        /// <exception cref="ProfileNotFoundException">is thrown if the profile does not exists</exception>
        /// <exception cref="StorageUnavailableException">the database is unavailable</exception>
        Task<Profile?> GetProfile(string username);
        /// <exception cref="ProfileNotFoundException">is thrown if the profile does not exists</exception>
        /// <exception cref="StorageUnavailableException">the database is unavailable</exception>
        Task DeleteProfile(string username);
    }
}
