using Azure.Storage.Blobs;
using ChatApp.Configuration;
using ChatApp.Dtos;
using ChatApp.Storage.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Net;

namespace ChatApp.Storage
{
    public class BlobImageStore : IImageStore
    {
        private readonly BlobContainerClient _containerClient;
        private readonly BlobSettings _blobSettings;


        public BlobImageStore(BlobServiceClient blobServiceClient, IOptions<BlobSettings> options)
        {
            _blobSettings = options.Value;
            _containerClient = blobServiceClient.GetBlobContainerClient(options.Value.ContainerName);
        }



        public async Task<byte[]> DownloadImage(string id)
        {
            var response = await _containerClient.GetBlobClient(id).DownloadAsync();
            await using var memoryStream = new MemoryStream();
            await response.Value.Content.CopyToAsync(memoryStream);
            var bytes=memoryStream.ToArray();
            return bytes;
        }
        public async Task<string> UploadImage(IFormFile file)
        {        
            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            string imageId = Guid.NewGuid().ToString();
            await _containerClient.UploadBlobAsync(imageId, stream);
            return imageId;
        }

    }
}
