using Azure;
using Azure.Core;
using Azure.Storage.Blobs;
using ChatApp.Configuration;
using ChatApp.Dtos;
using ChatApp.Exceptions;
using ChatApp.Storage.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace ChatApp.Storage
{
    public class BlobImageStore : IImageStore
    {
        private readonly BlobContainerClient _containerClient;

        public BlobImageStore(BlobServiceClient blobServiceClient, IOptions<BlobSettings> options)
        {
            _containerClient = blobServiceClient.GetBlobContainerClient(options.Value.ContainerName);
        }

        public async Task<string> UploadImage(byte[] file)
        {
           if(file==null) throw new ArgumentNullException(nameof(file));
            try
            {
                using var stream = new MemoryStream(file);
                string imageId = Guid.NewGuid().ToString();
                stream.Position = 0;
                await _containerClient.UploadBlobAsync(imageId, stream);
                return imageId;
            }
            catch(RequestFailedException e){
                throw new StorageUnavailableException($"Couldn't upload image to storage");
            }
        }

        public async Task<byte[]> DownloadImage(string id)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(id);
                var downloadContent = await blobClient.DownloadAsync();
                using (var memoryStream = new MemoryStream())
                {
                    await downloadContent.Value.Content.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (RequestFailedException e)
            {
                if (e.Status == 404) { throw new ImageNotFoundException(id); }
                throw new StorageUnavailableException($"Couldn't download image with id {id} from storage");
            }
        }

        public async Task DeleteImage(string id)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(id);
                await _containerClient.DeleteBlobAsync(id);
            }
            catch (RequestFailedException e)
            {
                if (e.Status == 404) { throw new ImageNotFoundException(id); }
                throw new StorageUnavailableException($"Couldn't delete image wiht id {id} from storage");
            }
        }

    }
}
