using Moq;
using System.Text;
using ChatApp.Storage;
using ChatApp.Exceptions;

namespace ChatApp.IntegrationTests
{
    public class ImageServiceTest
    {
        private readonly Mock<IImageStore> _imageStoreMock = new();
        private readonly Services.ImageService _imageService;

        public ImageServiceTest()
        {
            _imageService= new Services.ImageService(_imageStoreMock.Object);
        }

        [Fact]
        public async Task DownloadImage()
        {
            var imageId = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(imageId);
            _imageStoreMock.Setup(m => m.DownloadImage(imageId)).ReturnsAsync(bytes);

            var response = await _imageService.DownloadImage(imageId);
            _imageStoreMock.Verify(m => m.DownloadImage(imageId), Times.Once);
            Assert.Equal(bytes, response);
        }

        [Fact]
        public async Task DownloadImage_NotFound()
        {
            var imageId = Guid.NewGuid().ToString();
            _imageStoreMock.Setup(m => m.DownloadImage(imageId)).ThrowsAsync(new ImageNotFoundException(imageId));

            Assert.ThrowsAsync<ImageNotFoundException>(()=> _imageService.DownloadImage(imageId));
        }

        [Fact]
        public async Task DownloadImage_StorageUnavailable()
        {
            var imageId = Guid.NewGuid().ToString();
            _imageStoreMock.Setup(m => m.DownloadImage(imageId)).ThrowsAsync(new StorageUnavailableException(""));

            Assert.ThrowsAsync<StorageUnavailableException>(() => _imageService.DownloadImage(imageId));
        }

        [Fact]
        public async Task UploadImage()
        {
            var imageId = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(imageId);
            _imageStoreMock.Setup(m => m.UploadImage(bytes)).ReturnsAsync(imageId);

            var responseImageId = await _imageService.UploadImage(bytes);
            Assert.Equal(imageId, responseImageId);
            _imageStoreMock.Verify(m => m.UploadImage(bytes), Times.Once);
        }

        [Fact]
        public async Task UploadImage_Invalid()
        { 
            var imageId = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(imageId);
            _imageStoreMock.Setup(m => m.UploadImage(bytes)).ThrowsAsync(new ArgumentException(""));

            Assert.ThrowsAsync<ArgumentException>(() => _imageService.UploadImage(bytes));
        }

        [Fact]
        public async Task UploadImage_StorageUnavailable()
        {
            var imageId = Guid.NewGuid().ToString();
            var bytes = Encoding.UTF8.GetBytes(imageId);
            _imageStoreMock.Setup(m => m.UploadImage(bytes)).ThrowsAsync(new StorageUnavailableException(""));

            Assert.ThrowsAsync<StorageUnavailableException>(() => _imageService.UploadImage(bytes));
        }

        [Fact]
        public async Task DeleteImage()
        {
            var imageId = Guid.NewGuid().ToString();
            await _imageService.DeleteImage(imageId);

            _imageStoreMock.Verify(m => m.DeleteImage(imageId), Times.Once);
        }

        [Fact]
        public async Task DeleteImage_NotFound()
        {
            var imageId = Guid.NewGuid().ToString();
            _imageStoreMock.Setup(m => m.DeleteImage(imageId)).ThrowsAsync(new ImageNotFoundException(imageId));

            Assert.ThrowsAsync<ImageNotFoundException>(() => _imageService.DeleteImage(imageId));
        }

        [Fact]
        public async Task DeleteImage_StorageUnavailable()
        {
            var imageId = Guid.NewGuid().ToString();
            _imageStoreMock.Setup(m => m.DeleteImage(imageId)).ThrowsAsync(new StorageUnavailableException(""));

            Assert.ThrowsAsync<StorageUnavailableException>(() => _imageService.DeleteImage(imageId));
        }
    }
}
