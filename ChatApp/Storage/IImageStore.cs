using ChatApp.Dtos;
using System.Linq.Expressions;

namespace ChatApp.Storage
{
    public interface IImageStore
    {
        Task<string> UploadImage(byte[] file);
        Task<byte[]> DownloadImage(string id);
        Task DeleteImage(string id);
    }
}
