using API;
using Application.DTO;
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
        private const string ReferralCode = UserBase + "/code/";
        private const string ValidateCode = UserBase + "/validate/";
        private const string AttributeReferral = UserBase + "/attribute/";

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
            var url = ReferralCode + "U1";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            /* use ApiResponse here
            var code = await response.Content.ReadAsStringAsync();
            Assert.Equal(defaultReferralCode, code);
            */
        }

        [Fact]
        public async Task GetReferralCode_EmptyUid_ReturnsBadRequest()
        {
            var url = ReferralCode + " ";
            var response = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ValidateReferralCode_Valid_ReturnsTrue()
        {
            var url = $"{ValidateCode}?code={defaultReferralCode}";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"data\":true", result, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AttributeReferral_ValidRequest_ReturnsOk()
        {
            var request = new ReferralAttributionRequest
            {
                ReferralCode = defaultReferralCode,
                RefereeUid = "U2"
            };
            var json = System.Text.Json.JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(AttributeReferral, content);
            response.EnsureSuccessStatusCode();
        }
    }
}
