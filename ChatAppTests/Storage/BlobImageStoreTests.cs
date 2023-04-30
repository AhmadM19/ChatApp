using Azure.Core;
using Azure.Storage.Blobs;
using ChatApp.Configuration;
using ChatApp.Exceptions;
using ChatApp.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text;

namespace ChatApp.IntegrationTests
{


    public class BlobImageStoreTest 
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly IImageStore _imageStore;

        public BlobImageStoreTest()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();
            var blobSettings = new BlobSettings();
            configuration.GetSection("BlobStorage").Bind(blobSettings);
            _blobServiceClient = new BlobServiceClient(blobSettings.ConnectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(blobSettings.ContainerName);
            _imageStore = new BlobImageStore(_blobServiceClient, Options.Create(blobSettings));
        }


        [Fact]
        public async Task DownloadImage()
        {
            string str = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(str);
            var imageId = await _imageStore.UploadImage(bytes);
            var imageData = await _imageStore.DownloadImage(imageId);
            Assert.NotNull(imageData);
            //Dispose
            await _containerClient.DeleteBlobAsync(imageId);
        }

        [Fact]
        public async Task DownloadImage_NotFound()
        {
            var imageId =Guid.NewGuid().ToString();
            Assert.ThrowsAsync<ImageNotFoundException>(() => _imageStore.DownloadImage(imageId));
        }

        [Fact]
        public async Task UploadImage()
        {
            string str = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(str);
            var imageId = await _imageStore.UploadImage(bytes);
            Assert.NotNull(imageId);
            Assert.NotEmpty(imageId);
            //Dispose
            await _containerClient.DeleteBlobAsync(imageId);
        }

        [Fact]
        public async Task UploadImage_Invalid()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _imageStore.UploadImage(null));
        }

        [Fact]
        public async Task DeleteImage()
        {
            string str = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(str);
            var imageId = await _imageStore.UploadImage(bytes);
            await _imageStore.DeleteImage(imageId);
            var blobClient = _containerClient.GetBlobClient(imageId);
            Assert.False(await blobClient.ExistsAsync());
        }

        [Fact]
        public async Task DeleteImage_NotFound()
        {
            var imageId = Guid.NewGuid().ToString();
            Assert.ThrowsAsync<ImageNotFoundException>(() => _imageStore.DeleteImage(imageId));
        }


    }
}
