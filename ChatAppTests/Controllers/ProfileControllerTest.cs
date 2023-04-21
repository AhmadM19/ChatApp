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

namespace ChatAppTests.Controllers
{
    public class ProfileControllerTests: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly Mock<IProfileStore> _profileStoreMock = new();
        private readonly HttpClient _httpClient;

        public ProfileControllerTests(WebApplicationFactory<Program> factory)
        {
            _httpClient = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => { services.AddSingleton(_profileStoreMock.Object); });
            }).CreateClient();
        }

        [Fact]
        public async Task GetProfile()
        {
            var profile = new Profile("foobar", "Foo", "Bar", Guid.NewGuid().ToString());
            _profileStoreMock.Setup(m => m.GetProfile(profile.username))
                .ReturnsAsync(profile);

            var response = await _httpClient.GetAsync($"/Profile/{profile.username}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Equal(profile, JsonConvert.DeserializeObject<Profile>(json));
        }

        [Fact]
        public async Task GetProfile_Notfound()
        {
             _profileStoreMock.Setup(m => m.GetProfile("foobar"))
             .ReturnsAsync((Profile?)null);


            var response = await _httpClient.GetAsync($"/Profile/foobar");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task AddProfile()
        {
            var profile = new Profile("foobar", "Foo", "Bar", Guid.NewGuid().ToString());
            var response = await _httpClient.PostAsync("/Profile",
                new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("http://localhost/Profile/foobar", response.Headers.GetValues("Location").First());

            _profileStoreMock.Verify(mock => mock.UpsertProfile(profile), Times.Once);

        }

        [Fact]
        public async Task AddProfile_Conflict()
        {
            var profile = new Profile("foobar", "Foo", "Bar", Guid.NewGuid().ToString());
            //Mock the interface to return the same entity I am trying to add
            _profileStoreMock.Setup(m => m.GetProfile(profile.username))
    .ReturnsAsync(profile);

            var response = await _httpClient.PostAsync("/Profile",
                new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            _profileStoreMock.Verify(mock => mock.UpsertProfile(profile), Times.Never);
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
        [InlineData("foobar", "Foo", "Bar", null)]
        [InlineData("foobar", "Foo", "Bar", "")]
        [InlineData("foobar", "Foo", "Bar", " ")]
        public async Task AddProfile_InvalidArgs(string username, string firstName, string lastName, string profilePictureId)
        {
            var profile = new Profile(username, firstName, lastName, profilePictureId);
            var response = await _httpClient.PostAsync("/Profile",
    new StringContent(JsonConvert.SerializeObject(profile), Encoding.Default, "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            _profileStoreMock.Verify(mock => mock.UpsertProfile(profile), Times.Never);
        }

        [Fact]
        public async Task DeleteProfile()
        {
            var profile = new Profile("foobar", "Foo", "Bar", Guid.NewGuid().ToString());
            _profileStoreMock.Setup(m => m.GetProfile(profile.username))
            .ReturnsAsync(profile);
            var response = await _httpClient.DeleteAsync($"/Profile/{profile.username}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _profileStoreMock.Verify(mock => mock.DeleteProfile(profile.username), Times.Once);
        }
        [Fact]
        public async Task DeleteProfile_NotFound()
        {
            _profileStoreMock.Setup(m => m.GetProfile("foobar"))
            .ReturnsAsync((Profile?)null);
            var response = await _httpClient.DeleteAsync("/Profile/foobar");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            _profileStoreMock.Verify(mock => mock.DeleteProfile("foobar"), Times.Never);
        }


    }
}
