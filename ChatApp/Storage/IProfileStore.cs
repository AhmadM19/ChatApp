using ChatApp.Dtos;
using ChatApp.Exceptions;

namespace ChatApp.Storage
{
    public interface IProfileStore
    {
        /// <exception cref="ArgumentException">if arguments are not passed correctly</exception>
        /// <exception cref="StorageUnavailableException">the database is unavailable</exception>
        Task CreateProfile(Profile profile);

        /// <exception cref="StorageUnavailableException">the database is unavailable</exception>
        Task<Profile?> GetProfile(string username);

        /// <exception cref="StorageUnavailableException">the database is unavailable</exception>
        Task DeleteProfile(string username);
    }
}
