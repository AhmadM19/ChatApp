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
using ChatApp.Services;
using ChatApp.Exceptions;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;

namespace ChatAppTests.Controllers
{
    public class ImageControllerTests: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly Mock<IImageService> _imageServiceMock = new();
        private readonly HttpClient _httpClient;
      
        public ImageControllerTests(WebApplicationFactory<Program> factory)
        {
            _httpClient = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => { services.AddSingleton(_imageServiceMock.Object); });
            }).CreateClient(); 
        }

        [Fact]
        public async Task DownloadImage()
        {
            string str = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(str);
            string imageId = Guid.NewGuid().ToString();
            _imageServiceMock.Setup(m => m.DownloadImage(imageId)).ReturnsAsync(bytes);

            var response = await _httpClient.GetAsync($"api/images/{imageId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/png", response.Content.Headers.ContentType?.ToString());
        }

        [Fact]
        public async Task DownloadImage_NotFound()
        {
            string imageId = Guid.NewGuid().ToString();
            _imageServiceMock.Setup(m => m.DownloadImage(imageId)).ThrowsAsync(new ImageNotFoundException(imageId));

            var response = await _httpClient.GetAsync($"api/images/{imageId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DownloadImage_StorageUnavailable()
        {
            string str = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(str);
            string imageId = Guid.NewGuid().ToString();
            _imageServiceMock.Setup(m => m.DownloadImage(imageId)).
                ThrowsAsync(new StorageUnavailableException(imageId));

            var response = await _httpClient.GetAsync($"api/images/{imageId}");
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        [Fact]
        public async Task UploadImage()
        {
            string str = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(str);
            var stream = new MemoryStream(bytes);
            var imageId= Guid.NewGuid().ToString();
            HttpContent fileStreamContent = new StreamContent(stream);
            fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = "anything"
            };
            using var formData = new MultipartFormDataContent();
            formData.Add(fileStreamContent);
            _imageServiceMock.Setup(m => m.UploadImage(bytes)).ReturnsAsync(imageId);

            var response = await _httpClient.PostAsync("api/images", formData);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _imageServiceMock.Verify(mock => mock.UploadImage(bytes), Times.Once);
        }

        [Fact]
        public async Task UploadImage_Invalid()
        {
            var response= await _httpClient.PostAsync("api/images", null);
            Assert.Equal(HttpStatusCode.BadRequest,response.StatusCode);
        }

        [Fact]
        public async Task UploadImage_StorageUnavailable()
        {
            string str = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(str);
            var stream = new MemoryStream(bytes);
            string imageId = Guid.NewGuid().ToString();
            _imageServiceMock.Setup(m => m.UploadImage(bytes)).
                ThrowsAsync(new StorageUnavailableException(imageId));
            HttpContent fileStreamContent = new StreamContent(stream);
            fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = "anything"
            };
            using var formData = new MultipartFormDataContent();
            formData.Add(fileStreamContent);

            var response = await _httpClient.PostAsync($"api/images",formData);
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }

        [Fact]
        public async Task DeleteImage()
        {
            string imageId = Guid.NewGuid().ToString();

            var response = await _httpClient.DeleteAsync($"api/images/{imageId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _imageServiceMock.Verify(mock => mock.DeleteImage(imageId), Times.Once);
        }

        [Fact]
        public async Task DeleteImage_NotFound()
        {
            string imageId = Guid.NewGuid().ToString();
            _imageServiceMock.Setup(m => m.DeleteImage(imageId)).ThrowsAsync(new ImageNotFoundException(imageId));

            var response = await _httpClient.DeleteAsync($"api/images/{imageId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteImage_StorageUnavailable()
        {
            string str = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(str);
            string imageId = Guid.NewGuid().ToString();
            _imageServiceMock.Setup(m => m.DeleteImage(imageId)).
                ThrowsAsync(new StorageUnavailableException(imageId));

            var response = await _httpClient.DeleteAsync($"api/images/{imageId}");
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }
    }
}
