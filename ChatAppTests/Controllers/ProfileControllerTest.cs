using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using ChatApp.Storage;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using ChatApp.Dtos;
using ChatApp.Services;
using ChatApp.Exceptions;
using Microsoft.Azure.Cosmos;
using static System.Net.Mime.MediaTypeNames;

namespace ChatAppTests.Controllers
{
    public class ProfileControllerTests: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly Mock<IProfileService> _profileServiceMock = new();
        private readonly HttpClient _httpClient;

        public ProfileControllerTests(WebApplicationFactory<Program> factory)
        {
            _httpClient = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => { services.AddSingleton(_profileServiceMock.Object); });
            }).CreateClient();
        }

        [Fact]
        public async Task GetProfile()
        {
            var profile = new Profile("foobar", "Foo", "Bar", Guid.NewGuid().ToString());
            _profileServiceMock.Setup(m => m.GetProfile(profile.username))
                .ReturnsAsync(profile);

            var response = await _httpClient.GetAsync($"api/profile/{profile.username}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(profile, JsonConvert.DeserializeObject<Profile>(json));
        }

        [Fact]
        public async Task GetProfile_Notfound()
        {
            _profileServiceMock.Setup(m => m.GetProfile("foobar"))
             .ThrowsAsync(new ProfileNotFoundException("foobar"));

            var response = await _httpClient.GetAsync($"api/profile/foobar");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Get_Profile_StorageUnavailable()
        {
            _profileServiceMock.Setup(m => m.GetProfile("foo")).
                ThrowsAsync(new StorageUnavailableException("foo",null));

            var response = await _httpClient.GetAsync($"api/profile/foo");
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        }

        [Fact]
        public async Task CreateProfile()
        {
            var profile = new Profile("foobar", "Foo", "Bar", Guid.NewGuid().ToString());

            var response = await _httpClient.PostAsync("api/profile",
                new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _profileServiceMock.Verify(mock => mock.CreateProfile(profile), Times.Once);

        }

        [Fact]
        public async Task CreateProfile_Conflict()
        {
            var profile = new Profile("foobar", "Foo", "Bar", Guid.NewGuid().ToString());
            _profileServiceMock.Setup(m => m.CreateProfile(profile))
            .ThrowsAsync(new DuplicateProfileException(profile.username));

            var response = await _httpClient.PostAsync("api/profile",
            new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Theory]
        [InlineData(null, "Foo", "Bar","5b0fa492")]
        [InlineData("", "Foo", "Bar","5b0fa492")]
        [InlineData(" ", "Foo", "Bar", "5b0fa492")]
        [InlineData("foobar", null, "Bar", "5b0fa492")]
        [InlineData("foobar", "", "Bar","5b0fa492")]
        [InlineData("foobar", "  ", "Bar", "5b0fa492")]
        [InlineData("foobar", "Foo", null, "5b0fa492")]
        [InlineData("foobar", "Foo", "", "5b0fa492")]
        [InlineData("foobar", "Foo", " ", "5b0fa492")]
        public async Task CreateProfile_InvalidArgs(string username, string firstName, string lastName, string profilePictureId)
        {
            var profile = new Profile(username, firstName, lastName, profilePictureId);
            var response = await _httpClient.PostAsync("api/profile",
    new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _profileServiceMock.Verify(mock => mock.CreateProfile(profile), Times.Never);
        }

        [Fact]
        public async Task Create_Profile_StorageUnavailable()
        {
            var profile = new Profile("foobar", "Foo", "Bar", Guid.NewGuid().ToString());

            _profileServiceMock.Setup(m => m.CreateProfile(profile)).ThrowsAsync(new StorageUnavailableException("foo", null));
            var response = await _httpClient.PostAsync($"api/profile", 
                new StringContent(JsonConvert.SerializeObject(profile),Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        }

        [Fact]
        public async Task DeleteProfile()
        {
            var profile = new Profile("foobar", "Foo", "Bar", Guid.NewGuid().ToString());

            var response = await _httpClient.DeleteAsync($"api/profile/{profile.username}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _profileServiceMock.Verify(mock => mock.DeleteProfile(profile.username), Times.Once);
        }
        [Fact]
        public async Task DeleteProfile_NotFound()
        {
            _profileServiceMock.Setup(m => m.DeleteProfile("foobar"))
            .ThrowsAsync(new ProfileNotFoundException("foobar"));

            var response = await _httpClient.DeleteAsync("api/profile/foobar");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_Profile_StorageUnavailable()
        {
            _profileServiceMock.Setup(m => m.DeleteProfile("foo")).
                ThrowsAsync(new StorageUnavailableException("foo", null));

            var response = await _httpClient.DeleteAsync($"api/profile/foo");
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);

        }


    }
}
