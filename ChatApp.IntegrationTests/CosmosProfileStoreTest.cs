using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using ChatApp.Dtos;
using ChatApp.Storage;
using Microsoft.Azure.Cosmos;

namespace ProfileService.Web.IntegrationTests;

public class CosmosProfileStoreTest : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
{
    private readonly IProfileStore _store;

    private readonly Profile _profile = new(
        username: Guid.NewGuid().ToString(),
        firstName: "Foo",
        lastName: "Bar",
        profilePictureId: Guid.NewGuid().ToString()
    );

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _store.DeleteProfile(_profile.username);
    }

    public CosmosProfileStoreTest(WebApplicationFactory<Program> factory)
    {
        _store = factory.Services.GetRequiredService<IProfileStore>();
    }

    [Fact]
    public async Task AddProfile()
    {
        await _store.UpsertProfile(_profile);
        Assert.Equal(_profile, await _store.GetProfile(_profile.username));
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
    [InlineData("foobar", "Foo", "Bar", null)]
    [InlineData("foobar", "Foo", "Bar", "")]
    [InlineData("foobar", "Foo", "Bar", " ")]
    public async Task AddProfile_InvalidArgs(string username, string firstName, string lastName, string profilePictureId)
    {
        var invalidProfile=new Profile(username, firstName, lastName, profilePictureId);
        await Assert.ThrowsAsync<ArgumentException>(() =>  _store.UpsertProfile(invalidProfile));

    }

    [Fact]
    public async Task GetProfile()
    {
        await _store.UpsertProfile(_profile);
        var profile = await _store.GetProfile(_profile.username);
        Assert.NotNull(profile);
    }

    [Fact]
    public async Task GetProfile_NotFound()
    {
        Assert.Null(await _store.GetProfile(_profile.username));
    }

    [Fact]
    public async Task DeleteProfile()
    {
        await _store.UpsertProfile(_profile);
        await _store.DeleteProfile(_profile.username);
        Assert.Null(await _store.GetProfile(_profile.username));
    }

    [Fact]
    public async Task DeleteProfile_NotFound()
    {
        await _store.DeleteProfile(_profile.username);
        //Do nothing,method should not throw any exception if profile not found
    }
}
