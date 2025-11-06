using API;
using Application.DTO;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Tests
{
    public class ReferralsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient client;

        // ✅ Centralized endpoint definitions
        private const string BaseReferrals = "/api/referrals";
        private const string CodeByUid = "/api/referrals/code/";
        private const string ValidateCode = "/api/referrals/validate/";
        private const string ReferralListByUid = "/api/referrals/list/";
        private const string InviteMessage = "/api/referrals/invite-msg";
        private const string ReferralStats = "/api/referrals/stats";
        private const string AttributeReferral = "/api/attribute";

        public ReferralsControllerTests(WebApplicationFactory<Program> factory)
        {
            client = factory.CreateClient();
        }

        [Fact]
        public async Task GetReferralCode_ValidUid_ReturnsCode()
        {
            var response = await client.GetAsync(CodeByUid + "U1");
            response.EnsureSuccessStatusCode();
            var code = await response.Content.ReadAsStringAsync();
            Assert.Equal("\"ABC123\"", code);
        }

        [Fact]
        public async Task GetReferralCode_EmptyUid_ReturnsBadRequest()
        {
            var response = await client.GetAsync(CodeByUid + " ");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ValidateReferralCode_Valid_ReturnsTrue()
        {
            var response = await client.GetAsync(ValidateCode + "ABC123");
            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("true", result);
        }

        [Fact]
        public async Task GetReferrals_ValidUid_ReturnsList()
        {
            var response = await client.GetAsync(ReferralListByUid + "U1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("referral", content);
        }

        [Fact]
        public async Task GetReferrals_EmptyUid_ReturnsBadRequest()
        {
            var response = await client.GetAsync(ReferralListByUid + " ");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddReferral_ValidRequest_ReturnsTrue()
        {
            var request = new ReferralAddRequest
            {
                Uid = "U1",
                Name = "John",
                Method = ReferralMethod.EMAIL, 
                ReferralCode = "ABC123"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/referrals", content);
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
            var response = await client.GetAsync($"{InviteMessage}?method=email&referralCode=ABC123");
            response.EnsureSuccessStatusCode();
            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains("ABC123", message);
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
            var response = await client.GetAsync($"{BaseReferrals}?referralCode=ABC123&name=John");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));

            var referral = JsonSerializer.Deserialize<Referral>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(referral);
            Assert.Equal("ABC123", referral.ReferralCode);
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
            var json = """
            {
                "referralCode": "ABC123",
                "refereeUid": "U2"
            }
            """;
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(AttributeReferral, content);
            response.EnsureSuccessStatusCode();
        }
    }
}
