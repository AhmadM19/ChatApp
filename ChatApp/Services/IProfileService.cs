using ChatApp.Dtos;
using ChatApp.Exceptions;

namespace ChatApp.Services
{
    public interface IProfileService
    {

        /// <exception cref="ArgumentException">if arguments are not passed correctly</exception>
        /// <exception cref="DuplicateProfileException">if the profile already exists</exception>
        /// <exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task CreateProfile(Profile profile);
        /// <exception cref="ProfileNotFoundException">if the profile does not exists</exception>
        /// <exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task<Profile?> GetProfile(string username);
        /// <exception cref="ProfileNotFoundException"> if the profile does not exists</exception>
        /// <exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task DeleteProfile(string username);
    }
}
