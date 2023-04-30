using Microsoft.AspNetCore.Mvc.Testing;
using ProfileService.Web.IntegrationTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using ChatApp.Storage;
using ChatApp.Services;
using ChatApp.Dtos;
using ChatApp.Exceptions;

namespace ChatApp.IntegrationTests
{

    public class ProfileServiceTest
    {
        private readonly Mock<IProfileStore> _profileStoreMock=new();
        private readonly Services.ProfileService _profileService;
        public ProfileServiceTest()
        {
            _profileService = new Services.ProfileService(_profileStoreMock.Object);
        }

        [Fact]
        public async Task GetProfile()
        {
            var profile = new Profile("foobar", "Foo", "Bar", Guid.NewGuid().ToString());
            _profileStoreMock.Setup(m => m.GetProfile(profile.username)).ReturnsAsync(profile);

            var result =   await _profileService.GetProfile(profile.username);
            Assert.Equivalent(profile, result, true);
        }

        [Fact]
        public async Task GetProfile_NotFound()
        {
           _profileStoreMock.Setup(m => m.GetProfile("foobar")).ReturnsAsync((Profile?)null);

            await Assert.ThrowsAsync<ProfileNotFoundException>(() =>_profileService.GetProfile("foobar"));
        }

        [Fact]
        public async Task CreateProfile()
        {
            var profile = new Profile("foobar", "Foo", "Bar", Guid.NewGuid().ToString());
            _profileStoreMock.Setup(m => m.GetProfile(profile.username)).ReturnsAsync((Profile?)null);

            await _profileService.CreateProfile(profile);
            _profileStoreMock.Verify(mock => mock.CreateProfile(profile),Times.Once);
        }

        [Fact]
        public async Task CreateProfile_Duplicate()
        {
            var profile = new Profile("foobar", "Foo", "Bar", Guid.NewGuid().ToString());
            _profileStoreMock.Setup(m => m.GetProfile(profile.username)).ReturnsAsync(profile);

            await Assert.ThrowsAsync<DuplicateProfileException>(()=>_profileService.CreateProfile(profile));
        }

        [Fact]
        public async Task DeleteProfile()
        {
            var profile = new Profile("foobar", "Foo", "Bar", Guid.NewGuid().ToString());
            _profileStoreMock.Setup(m => m.GetProfile(profile.username)).ReturnsAsync(profile);

            await _profileService.DeleteProfile("foobar");
            _profileStoreMock.Verify(mock => mock.DeleteProfile("foobar"), Times.Once);
        }

        [Fact]
        public async Task DeleteProfile_NotFound()
        {
            _profileStoreMock.Setup(m => m.GetProfile("foobar")).ReturnsAsync((Profile?)null);
            await Assert.ThrowsAsync<ProfileNotFoundException>(() => _profileService.DeleteProfile("foobar"));
        }
    }
 }