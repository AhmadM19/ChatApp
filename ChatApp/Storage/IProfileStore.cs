using ChatApp.Dtos;

namespace ChatApp.Storage
{
    public interface IProfileStore
    {
        Task UpsertProfile(Profile profile);
        Task<Profile?> GetProfile(string username);
        Task DeleteProfile(string username);
    }
}
