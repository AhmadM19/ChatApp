using ChatApp.Storage;

namespace ChatApp.Services
{
    public class ImageService:IImageService
    {
        private readonly IImageStore _imageStore;

        public ImageService(IImageStore imageStore)
        {
            _imageStore = imageStore;
        }

        public Task DeleteImage(string id)
        {
            return _imageStore.DeleteImage(id);
        }

        public Task<byte[]> DownloadImage(string id)
        {
            return _imageStore.DownloadImage(id);
        }

        public Task<string> UploadImage(byte[] file)
        {
            return _imageStore.UploadImage(file);
        }
    }
}
