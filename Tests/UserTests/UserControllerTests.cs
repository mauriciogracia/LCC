using API;
using Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using Tests;

namespace UserTests
{
    public class UserControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient client;

        // USER endpoint URLs 
        private const string UserBase = "/api/user";
        private const string CodeByUid = UserBase + "/code";
        private const string ValidateCode = UserBase + "/validate";
        private const string AttributeReferral = UserBase + "/attribute";

        private const string defaultUid = "U1";
        private const string defaultReferralCode = "NAQXC0";

        public UserControllerTests()
        {
            var factory = new CustomWebApplicationFactory();
            client = factory.CreateClient();
        }

        [Fact]
        public void SeededUsers_ArePresent()
        {
            // Arrange
            var factory = new CustomWebApplicationFactory();
            using var scope = factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ReferralDbContext>();

            // Act
            var users = context.Users.ToList();

            // Assert
            Assert.Equal(5, users.Count);
            Assert.Contains(users, u => u.Uid == defaultUid && u.Name == "María Gómez");
            Assert.Contains(users, u => u.Uid == "U5" && u.Email == "sofia.ramirez@mgg.com");
        }

        [Fact]
        public async Task GetReferralCode_ValidUid_ReturnsCode()
        {
            var response = await client.GetAsync(CodeByUid + "U1");
            response.EnsureSuccessStatusCode();
            var code = await response.Content.ReadAsStringAsync();
            Assert.Equal(defaultReferralCode, code);
        }

        [Fact]
        public async Task GetReferralCode_EmptyUid_ReturnsBadRequest()
        {
            var response = await client.GetAsync(CodeByUid + " ");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ValidateReferralCode_Valid_ReturnsTrue()
        {
            var response = await client.GetAsync(ValidateCode + defaultReferralCode);
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("true", result);
        }
        
        [Fact]
        public async Task AttributeReferral_ValidRequest_ReturnsOk()
        {
            var json = $$"""
            {
                "referralCode": "{{defaultReferralCode}}",
                "refereeUid": "U2"
            }
            """;
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(AttributeReferral, content);
            response.EnsureSuccessStatusCode();
        }
    }
}
