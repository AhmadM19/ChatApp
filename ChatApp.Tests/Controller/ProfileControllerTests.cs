
using Microsoft.AspNetCore.Mvc.Testing;
using ChatApp.Storage;
using Moq;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ChatApp.Dtos;
using System.Net;
using Newtonsoft.Json;
using System.Text;

namespace ChatApp.Tests.Controller
{
    public class ProfileControllerTests : IClassFixture<WebApplicationFactory<Program>>
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
    }
}