using API;
using Application.DTO;
using Domain.Entities;
using Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Tests
{
    public class ReferralsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient client;

        // endpoint URLs
        private const string BaseReferrals = "/api/referrals";
        private const string CodeByUid = "/api/referrals/code/";
        private const string ValidateCode = "/api/referrals/validate/";
        private const string ReferralListByUid = "/api/referrals/list/";
        private const string InviteMessage = "/api/referrals/invite-msg";
        private const string ReferralStats = "/api/referrals/stats";
        private const string AttributeReferral = "/api/attribute";

        private const string defaultUid = "U1";
        private const string defaultReferralCode = "NAQXC0";

        public ReferralsControllerTests()
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
        public async Task GetReferrals_ReturnsEmptyList()
        {
            var response = await client.GetAsync(ReferralListByUid + defaultUid + "X");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.True(content == "[]");
        }

        private async Task<string> utilGetReferrals(string uid)
        {
            var response = await client.GetAsync(ReferralListByUid + uid);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        [Fact]
        public async Task GetReferrals_ReturnsReferralsList()
        {
            var content = await utilGetReferrals(defaultUid);
            Assert.True(content == "[]");

            var response = await utilddReferral(defaultUid, "Alice", ReferralMethod.EMAIL);
            response.EnsureSuccessStatusCode();

            content = await utilGetReferrals(defaultUid);
            Assert.Contains("Alice", content);

            response = await utilddReferral(defaultUid, "Joseph", ReferralMethod.EMAIL);
            response.EnsureSuccessStatusCode();

            content = await utilGetReferrals(defaultUid);
            Assert.Contains("Alice", content);
            Assert.Contains("Joseph", content);
        }


        [Fact]
        public async Task GetReferrals_EmptyUid_ReturnsBadRequest()
        {
            var response = await client.GetAsync(ReferralListByUid + " ");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<System.Net.Http.HttpResponseMessage> utilddReferral(string uid, string name, ReferralMethod method)
        {
            var request = new ReferralAddRequest
            {
                Uid = uid,
                Name = name,
                Method = method,
                ReferralCode = defaultReferralCode
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await client.PostAsync("/api/referrals", content);
        }

        [Fact]
        public async Task AddReferral_ValidRequest_ReturnsTrue()
        {
            var response = await utilddReferral(defaultUid, "Carlos", ReferralMethod.EMAIL);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("true", result);
        }

        [Fact]
        public async Task AddReferral_InvalidRequest_ReturnsBadRequest()
        {
            var json = "{}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(BaseReferrals, content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PrepareMessage_ValidCode_ReturnsMessage()
        {
            var response = await client.GetAsync($"{InviteMessage}?method=EMAIL&referralCode=" + defaultReferralCode);
            response.EnsureSuccessStatusCode();
            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains(defaultReferralCode, message);
        }

        [Fact]
        public async Task PrepareMessage_InvalidCode_ReturnsBadRequest()
        {
            var response = await client.GetAsync($"{InviteMessage}?method=email&referralCode=INVALID");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetReferral_NotFound_ReturnsNotFound()
        {
            var response = await client.GetAsync($"{BaseReferrals}?referralCode=ABC999&name=Ghost");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetReferral_ValidRequest_ReturnsReferral()
        {
            var addResponse = await utilddReferral(defaultUid, "John", ReferralMethod.EMAIL);
            addResponse.EnsureSuccessStatusCode();

            var response = await client.GetAsync($"{BaseReferrals}?referralCode="+ defaultReferralCode + "&name=John");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));

            var referral = JsonSerializer.Deserialize<Referral>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(referral);
            Assert.Equal(defaultReferralCode, referral.ReferralCode);
        }


        [Fact]
        public async Task GetReferral_InvalidCode_ReturnsBadRequest()
        {
            var response = await client.GetAsync($"{BaseReferrals}?referralCode=INVALID&name=John");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateReferral_ValidRequest_ReturnsOk()
        {
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{BaseReferrals}?referralCode=ABC123&name=John&status=active", content);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetReferralStats_ValidUid_ReturnsStats()
        {
            var response = await client.GetAsync($"{ReferralStats}?uid=U1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("total", content);
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
