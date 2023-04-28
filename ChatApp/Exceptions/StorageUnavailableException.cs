using Microsoft.Azure.Cosmos;

namespace ChatApp.Exceptions
{
    public class StorageUnavailableException : Exception
    {
        public StorageUnavailableException(string? message) : base(message)
        {
        }
    }
}
