using API;
using Application.DTO;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Text.Json;
using Tests;

namespace ReferralTests
{
    public class ReferralsControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient client;

        // endpoint URLs
        private const string ReferralsBase = "/api/referrals";

        private const string ReferralsPostfix = "referrals";
        private const string InviteMessage = ReferralsBase + "/invite-message/";
        private const string ReferralStats = ReferralsBase + "/statistics/";

        private const string defaultUid = "U1";
        private const string defaultReferralCode = "NAQXC0";

        public ReferralsControllerTests()
        {
            var factory = new CustomWebApplicationFactory();
            client = factory.CreateClient();
        }

        private string prepareListReferralsURL(string uid)
        {
            return $"{ ReferralsBase}/{uid}/{ReferralsPostfix}";
        }

        [Fact]
        public async Task GetReferrals_ReturnsEmptyList()
        {
            string url = prepareListReferralsURL(defaultUid + "X");
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.True(content == "[]");
        }

        private async Task<string> utilGetReferrals(string uid)
        {
            string url = prepareListReferralsURL(uid);
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        [Fact]
        public async Task GetReferrals_ReturnsReferralsList()
        {
            var content = await utilGetReferrals(defaultUid);
            Assert.True(content == "[]");

            var response = await utilCreateReferral(defaultUid, "Alice", ReferralMethod.EMAIL);
            response.EnsureSuccessStatusCode();

            content = await utilGetReferrals(defaultUid);
            Assert.Contains("Alice", content);

            response = await utilCreateReferral(defaultUid, "Joseph", ReferralMethod.EMAIL);
            response.EnsureSuccessStatusCode();

            content = await utilGetReferrals(defaultUid);
            Assert.Contains("Alice", content);
            Assert.Contains("Joseph", content);
        }

        private async Task<HttpResponseMessage> utilCreateReferral(string uid, string name, ReferralMethod method)
        {
            var request = new ReferralAddRequest
            {
                Uid = uid,
                Name = name,
                Method = method,
                ReferralCode = defaultReferralCode
            };

            var json = System.Text.Json.JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await client.PostAsync("/api/referrals", content);
        }

        [Fact]
        public async Task AddReferral_ValidRequest_ReturnsTrue()
        {
            var response = await utilCreateReferral(defaultUid, "Carlos", ReferralMethod.EMAIL);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            Assert.Equal("true", result);
        }

        [Fact]
        public async Task AddReferral_InvalidRequest_ReturnsBadRequest()
        {
            var json = "{}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(ReferralsBase, content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PrepareMessage_ValidCode_ReturnsMessage()
        {
            string url = $"{InviteMessage}?method=EMAIL&referralCode=" + defaultReferralCode;
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains(defaultReferralCode, message);
        }

        [Fact]
        public async Task PrepareMessage_InvalidCode_ReturnsBadRequest()
        {
            string url = $"{InviteMessage}?method=email&referralCode=INVALID";
            var response = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetReferral_NotFound_ReturnsNotFound()
        {
            string url = $"{ReferralsBase}?referralCode=ABC999&name=Ghost";
            var response = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetReferral_ValidRequest_ReturnsReferral()
        {
            var addResponse = await utilCreateReferral(defaultUid, "John", ReferralMethod.EMAIL);
            addResponse.EnsureSuccessStatusCode();

            var response = await client.GetAsync($"{ReferralsBase}?referralCode=" + defaultReferralCode + "&name=John");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));

            var referral = System.Text.Json.JsonSerializer.Deserialize<Referral>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(referral);
            Assert.Equal(defaultReferralCode, referral.ReferralCode);
        }


        [Fact]
        public async Task GetReferral_InvalidCode_ReturnsBadRequest()
        {
            var response = await client.GetAsync($"{ReferralsBase}?referralCode=INVALID&name=John");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task UpdateReferral_ValidRequest_ReturnsOk()
        {
            // Arrange: build the request object
            var request = new ReferralUpdateRequest
            {
                Id = "123456",          // still present in DTO
                ReferralCode = "ABC123",
                Name = "John",
                Status = "active"
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act: send PUT with referralId in route
            string referralId = request.Id; // matches controller route param
            string url = $"{ReferralsBase}/{referralId}";
            var response = await client.PutAsync(url, content);

            // Assert: ensure success
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetReferralStats_ValidUid_ReturnsStats()
        {
            string url = $"{ReferralStats}?uid=U1";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("total", content);
        }
    }
}
