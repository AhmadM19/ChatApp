using ChatApp.Dtos;

namespace ChatApp.Services
{
    public interface IProfileService
    {

        /// <exception cref="ArgumentException">Arguments are not passed correctly</exception>
        /// <exception cref="StorageUnavailableException">the database is unavailable</exception>
        /// <exception cref="DuplicatProfileException">the profile already exists</exception>
        Task CreateProfile(Profile profile);
        /// <exception cref="ProfileNotFoundException">is thrown if the profile does not exists</exception>
        /// <exception cref="StorageUnavailableException">the database is unavailable</exception>
        Task<Profile?> GetProfile(string username);
        /// <exception cref="StorageUnavailableException">the database is unavailable</exception>
        /// <exception cref="ProfileNotFoundException">is thrown if the profile does not exists</exception>
        Task DeleteProfile(string username);
    }
}
