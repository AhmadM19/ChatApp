namespace ChatApp.Services
{
    public interface IImageService
    {
        Task<string> UploadImage(byte[] file);
        Task<byte[]> DownloadImage(string id);
        Task DeleteImage(string id);
    }
}
