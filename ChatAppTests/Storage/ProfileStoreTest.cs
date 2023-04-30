using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using ChatApp.Dtos;
using ChatApp.Storage;
using Microsoft.Azure.Cosmos;
using ChatApp.Exceptions;

namespace ProfileService.Web.IntegrationTests;

public class ProfileStoreTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly IProfileStore _profileStore;

    private readonly Profile _profile = new(
        username: Guid.NewGuid().ToString(),
        firstName: "Foo",
        lastName: "Bar",
        profilePictureId: Guid.NewGuid().ToString()
    );

    public ProfileStoreTest(WebApplicationFactory<Program> factory)
    {
        _profileStore = factory.Services.GetRequiredService<IProfileStore>();
    }

    [Fact]
    public async Task CreateProfile()
    {
        await _profileStore.CreateProfile(_profile);
        Assert.Equal(_profile, await _profileStore.GetProfile(_profile.username));
        await _profileStore.DeleteProfile(_profile.username);
    }

    [Theory]
    [InlineData(null, "Foo", "Bar", "5b0fa492")]
    [InlineData("", "Foo", "Bar", "5b0fa492")]
    [InlineData(" ", "Foo", "Bar", "5b0fa492")]
    [InlineData("foobar", null, "Bar", "5b0fa492")]
    [InlineData("foobar", "", "Bar", "5b0fa492")]
    [InlineData("foobar", "  ", "Bar", "5b0fa492")]
    [InlineData("foobar", "Foo", null, "5b0fa492")]
    [InlineData("foobar", "Foo", "", "5b0fa492")]
    [InlineData("foobar", "Foo", " ", "5b0fa492")]
    public async Task CreateProfile_InvalidArgs(string username, string firstName, string lastName, string profilePictureId)
    {
        var invalidProfile=new Profile(username, firstName, lastName, profilePictureId);
        await Assert.ThrowsAsync<ArgumentException>(() => _profileStore.CreateProfile(invalidProfile));

    }

    [Fact]
    public async Task GetProfile()
    {
        await _profileStore.CreateProfile(_profile);
        var profile = await _profileStore.GetProfile(_profile.username);
        Assert.Equal(profile.username,_profile.username);
        Assert.Equal(profile.firstName, _profile.firstName);
        Assert.Equal(profile.lastName, _profile.lastName);
        await _profileStore.DeleteProfile(_profile.username);
    }

    [Fact]
    public async Task DeleteProfile()
    {
        await _profileStore.CreateProfile(_profile);
        await _profileStore.DeleteProfile(_profile.username);
        Assert.Null(await _profileStore.GetProfile(_profile.username));
    }

    //GetProfile_NotFound, DeleteProfile_NotFound, CreateProfile_Duplicate edge cases are checked at service layer
}
