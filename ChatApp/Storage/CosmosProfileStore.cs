using System.Net;
using Microsoft.Azure.Cosmos;
using ChatApp.Dtos;
using ChatApp.Storage.Entities;
using Microsoft.Extensions.Options;
using ChatApp.Configuration;
using ChatApp.Exceptions;

namespace ChatApp.Storage
{
    public class CosmosProfileStore: IProfileStore
    {
        private readonly CosmosClient _cosmosClient;

        public CosmosProfileStore(IOptions<CosmosSettings> options)
        {
            _cosmosClient = new CosmosClient(options.Value.ConnectionString);
        }

        private Container Container => _cosmosClient.GetDatabase("chatapp").GetContainer("profile");
        private static ProfileEntity ToEntity(Profile profile)
        {
            return new ProfileEntity(
                partitionKey: profile.username,
                id: profile.username,
                profile.firstName,
                profile.lastName,
                profile.profilePictureId);
        }
        private static Profile ToProfile(ProfileEntity entity)
        {
            return new Profile(
                username:entity.id,
                firstName:entity.firstName,
                lastName: entity.lastName,
                profilePictureId:entity.profilePictureId);
        }



        public async Task<Profile?> GetProfile(string username)
        {

            try
            {
                var entity = await Container.ReadItemAsync<ProfileEntity>(
                    id: username, partitionKey: new PartitionKey(username), 
                    new ItemRequestOptions { ConsistencyLevel=ConsistencyLevel.Session} );

                return ToProfile(entity);
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound) { return null; }
                throw new StorageUnavailableException
                    ($"Couldn't get profile with username {username} from storage",e) ;
            }
        }

        public async Task CreateProfile(Profile profile)
        {
            if (profile == null || string.IsNullOrWhiteSpace(profile.username) || string.IsNullOrWhiteSpace(profile.firstName)
               || string.IsNullOrWhiteSpace(profile.lastName))
            {
                throw new ArgumentException($"Invalid profile{profile}", nameof(profile));
            }
            try
            {
                await Container.CreateItemAsync(ToEntity(profile));
            }

            catch (CosmosException e)
            {
                throw new StorageUnavailableException($"Couldn't add profile with username {profile.username} to storage", e);
            }

        }
        public async Task DeleteProfile(string username)
        {

            try
            {
                await Container.DeleteItemAsync<Profile>(
                    id: username, partitionKey: new PartitionKey(username)
                );
            }
            catch(CosmosException e)
            {
                throw new StorageUnavailableException
                    ($"Couldn't delete profile with username {username} from storage", e);
            }
        }
    }
}
