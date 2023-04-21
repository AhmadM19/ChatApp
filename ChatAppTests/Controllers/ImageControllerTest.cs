using Azure;
using ChatApp.Dtos;
using ChatApp.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChatAppTests.Controllers
{
    public class ImageControllerTests: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly Mock<IImageStore> _imageStoreMock = new();
        private readonly HttpClient _httpClient;
      
        public ImageControllerTests(WebApplicationFactory<Program> factory)
        {
            _httpClient = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => { services.AddSingleton(_imageStoreMock.Object); });
            }).CreateClient(); 
        }

        [Fact]
        public async Task DownloadImage()
        {
            var imageFile = File.OpenRead("TestImages/avatarProfile.jpg");
            var imageStream = new MemoryStream();
            imageFile.CopyTo(imageStream);

            string imageId = Guid.NewGuid().ToString();

            _imageStoreMock.Setup(m => m.DownloadImage(imageId)).ReturnsAsync(imageStream.ToArray());
            var response = await _httpClient.GetAsync($"Image/{imageId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/png", response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task DownloadImage_NotFound()
        {
            string imageId = Guid.NewGuid().ToString();
            _imageStoreMock.Setup(m => m.DownloadImage(imageId)).ReturnsAsync((byte[]?)null);
            var response = await _httpClient.GetAsync($"Image/{imageId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UploadImage()
        {
            string str = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(str);
            var stream = new MemoryStream(bytes);

            HttpContent fileStreamContent = new StreamContent(stream);
            fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = "anything"
            };

            using var formData = new MultipartFormDataContent();
            formData.Add(fileStreamContent);

            await _httpClient.PostAsync("/Image", formData);

            _imageStoreMock.Verify(m => m.UploadImage(bytes));
        }

        [Fact]
        public async Task DeleteImage()
        {
            var imageFile = File.OpenRead("TestImages/avatarProfile.jpg");
            var imageStream = new MemoryStream();
            imageFile.CopyTo(imageStream);

            string imageId = Guid.NewGuid().ToString();

            _imageStoreMock.Setup(m => m.DownloadImage(imageId)).ReturnsAsync(imageStream.ToArray());

            var response = await _httpClient.DeleteAsync($"Image/{imageId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _imageStoreMock.Verify(mock => mock.DeleteImage(imageId), Times.Once);
        }

      
    }
}
