using Azure.Core;
using Azure.Storage.Blobs;
using ChatApp.Configuration;
using ChatApp.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChatApp.IntegrationTests
{


    public class BlobImageStoreTest 
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly IImageStore _imageStore;

        public BlobImageStoreTest()
        {
            // Read the BlobStorage section from the appsettings.Test.json file
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();
            var blobSettings = new BlobSettings();
            configuration.GetSection("BlobStorage").Bind(blobSettings);
            // Initialize the BlobServiceClient using the connection string from the configuration
            _blobServiceClient = new BlobServiceClient(blobSettings.ConnectionString);
            // Create the test container in the storage account
            _containerClient = _blobServiceClient.GetBlobContainerClient(blobSettings.ContainerName);
            // Create a new BlobImageStore object using the test container
            _imageStore = new BlobImageStore(_blobServiceClient, Options.Create(blobSettings));
        }


        [Fact]
        public async Task DownloadImage()
        {
            var fileName = "avatarProfile-1.jpg";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(),"TestImages",fileName);
            var file = new FormFile(new FileStream(filePath, FileMode.Open), 0, new FileInfo(filePath).Length, fileName, fileName);
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var imageId = await _imageStore.UploadImage(stream.ToArray());
            var imageData = await _imageStore.DownloadImage(imageId);
            Assert.NotNull(imageData);
            await _containerClient.DeleteBlobAsync(imageId);
        }

        [Fact]
        public async Task UploadImage()
        {
            var fileName = "avatarProfile-2.jpg";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestImages", fileName);
            var file = new FormFile(new FileStream(filePath, FileMode.Open), 0, new FileInfo(filePath).Length, fileName, fileName);
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var imageId = await _imageStore.UploadImage(stream.ToArray());
            Assert.NotNull(imageId);
            Assert.NotEmpty(imageId);
            await _containerClient.DeleteBlobAsync(imageId);
        }

        [Fact]
        public async Task DeleteImage()
        {
            var fileName = "avatarProfile-3.jpg";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "TestImages", fileName);
            var file = new FormFile(new FileStream(filePath, FileMode.Open), 0, new FileInfo(filePath).Length, fileName, fileName);
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            var imageId = await _imageStore.UploadImage(stream.ToArray());
            await _imageStore.DeleteImage(imageId);
            var blobClient = _containerClient.GetBlobClient(imageId);
            Assert.False(await blobClient.ExistsAsync());
        }
     

    }
}
