using Azure.Core;
using Azure.Storage.Blobs;
using ChatApp.Configuration;
using ChatApp.Dtos;
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

        public async Task<byte[]> DownloadImage(string id)
        {
            var blobClient = _containerClient.GetBlobClient(id);
            var downloadContent = await blobClient.DownloadAsync();
            using (var memoryStream = new MemoryStream())
            {
                await downloadContent.Value.Content.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public async Task<string> UploadImage(byte[] file)
        {
           using var stream = new MemoryStream(file);
           string imageId = Guid.NewGuid().ToString();
           stream.Position = 0;
           await _containerClient.UploadBlobAsync(imageId, stream);
           return imageId;   
        }

        public async Task DeleteImage(string id)
        {
            await _containerClient.DeleteBlobAsync(id);
        }

    }
}
