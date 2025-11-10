using API;
using Application.DTO;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
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

        private const string ReferralListByUid = ReferralsBase + "/list";
        private const string InviteMessage = ReferralsBase + "/invite-msg";
        private const string ReferralStats = ReferralsBase + "/stats";

        private const string defaultUid = "U1";
        private const string defaultReferralCode = "NAQXC0";

        public ReferralsControllerTests()
        {
            var factory = new CustomWebApplicationFactory();
            client = factory.CreateClient();
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


        [Fact]
        public async Task GetReferrals_EmptyUid_ReturnsBadRequest()
        {
            var response = await client.GetAsync(ReferralListByUid + " ");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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

            var json = JsonSerializer.Serialize(request);
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
            var response = await client.GetAsync($"{ReferralsBase}?referralCode=ABC999&name=Ghost");
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

            var referral = JsonSerializer.Deserialize<Referral>(
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
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"{ReferralsBase}?referralCode=ABC123&name=John&status=active", content);
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
    }
}
