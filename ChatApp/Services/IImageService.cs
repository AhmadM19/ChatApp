using ChatApp.Exceptions;

namespace ChatApp.Services
{
    public interface IImageService
    {
        /// <exception cref="ArgumentNullException">if uploaded image is null</exception>
        /// <exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task<string> UploadImage(byte[] file);
        /// <exception cref="ImageNotFoundException"> if the image does not exists</exception>
        /// <exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task<byte[]> DownloadImage(string id);
        /// <exception cref="ImageNotFoundException"> if the image does not exists</exception>
        /// <exception cref="StorageUnavailableException">if the database is unavailable</exception>
        Task DeleteImage(string id);
    }
}
