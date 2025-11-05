using API.Controllers;
using Application.DTO;
using Application.Interfaces;
using Domain;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests
{
    
    public class ReferralsControllerTests
    {
        readonly ReferralsController controller;
        readonly Mock<IReferralFeatures> referralMock = new();
        readonly Mock<IUtilFeatures> utilMock = new();
        readonly Mock<ILog> logMock = new();
        string defaultUid = "U1";

        public ReferralsControllerTests()
        {
            controller = new ReferralsController(referralMock.Object, utilMock.Object, logMock.Object);
        }

        // GetReferralCode: valid uid
        [Fact]
        public async Task GetReferralCode_ValidUid_ReturnsCode()
        {
            string uid = defaultUid;
            referralMock.Setup(r => r.GetUserReferralCode(uid)).ReturnsAsync("ABC123");

            var result = await controller.GetReferralCode(uid);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal("ABC123", ok.Value);
        }

        // GetReferralCode: check empty uid
        [Fact]
        public async Task GetReferralCode_EmptyUid_ReturnsBadRequest()
        {
            var result = await controller.GetReferralCode("");
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // ValidateReferralCode with valid code
        [Fact]
        public void ValidateReferralCode_Valid_ReturnsTrue()
        {
            utilMock.Setup(u => u.IsValidReferralCode("A1B2C3")).Returns(true);
            Assert.True(controller.ValidateReferralCode("A1B2C3"));
        }

        // ValidateReferralCode with invalid code
        [Fact]
        public void ValidateReferralCode_Invalid_ReturnsFalse()
        {
            utilMock.Setup(u => u.IsValidReferralCode("XYZ")).Returns(false);
            Assert.False(controller.ValidateReferralCode("XYZ"));
        }

        // GetReferrals with a valid uid
        [Fact]
        public async Task GetReferrals_ValidUid_ReturnsList()
        {
            referralMock.Setup(r => r.GetUserReferrals(defaultUid)).ReturnsAsync([new(defaultUid, "Jose", ReferralMethod.EMAIL, "A1B2C3")]);

            var result = await controller.GetReferrals(defaultUid);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<Referral>>(ok.Value);
            Assert.Single(list);
        }

        //  GetReferrals validate a null uid
        [Fact]
        public async Task GetReferrals_NullUid_ReturnsBadRequest()
        {
            var result = await controller.GetReferrals("");
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // AddReferral validate when correct request
        [Fact]
        public async Task AddReferral_ValidRequest_ReturnsTrue()
        {
            var req = new ReferralAddRequest { Uid = defaultUid, Name = "Jose", Method = ReferralMethod.EMAIL, ReferralCode = "A1B2C3" };
            utilMock.Setup(u => u.IsValidReferralCode(req.ReferralCode)).Returns(true);
            referralMock.Setup(r => r.AddReferral(It.IsAny<Referral>())).ReturnsAsync(true);

            var result = await controller.AddReferral(req);
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            //Since is a possible null value
            Assert.True(ok.Value is bool b && b);
        }

        // AddReferral validate invalid referral code
        [Fact]
        public async Task AddReferral_InvalidCode_ReturnsBadRequest()
        {
            var req = new ReferralAddRequest { Uid = defaultUid, Name = "Jose", Method = ReferralMethod.EMAIL, ReferralCode = "XYZ" };
            utilMock.Setup(u => u.IsValidReferralCode(req.ReferralCode)).Returns(false);

            var result = await controller.AddReferral(req);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // PrepareMessage validate correct referral code when preparing a message
        [Fact]
        public void PrepareMessage_Valid_ReturnsMessage()
        {
            var req = new PrepareMessageRequest { Method = ReferralMethod.EMAIL, ReferralCode = "A1B2C3" };
            utilMock.Setup(u => u.IsValidReferralCode(req.ReferralCode)).Returns(true);
            utilMock.Setup(u => u.PrepareMessage(req.Method, req.ReferralCode)).Returns("Invite via Email");

            var result = controller.PrepareMessage(req);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal("Invite via Email", ok.Value);
        }

        // PrepareMessage validate INCORRECT referral code when preparing a message
        [Fact]
        public void PrepareMessage_InvalidCode_ReturnsBadRequest()
        {
            var req = new PrepareMessageRequest { Method = ReferralMethod.EMAIL, ReferralCode = "XYZ" };
            utilMock.Setup(u => u.IsValidReferralCode(req.ReferralCode)).Returns(false);

            var result = controller.PrepareMessage(req);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // UpdateReferral check that a referral is updated
        [Fact]
        public async Task UpdateReferral_Valid_ReturnsTrue()
        {
            var req = new UpdateReferralRequest { ReferralCode = "A1B2C3", Name = "Jose", Status = "Completed" };
            referralMock.Setup(r => r.UpdateReferral(req.ReferralCode, req.Name, ReferralStatus.Completed)).ReturnsAsync(true);

            var result = await controller.UpdateReferral(req);
            var ok = Assert.IsType<OkObjectResult>(result.Result);

            //Since is a possible null value
            Assert.True(ok.Value is bool b && b);
        }

        // GetReferral check valid referral
        [Fact]
        public async Task GetReferral_Valid_ReturnsReferral()
        {
            var req = new GetReferralRequest { ReferralCode = "A1B2C3", Name = "Jose" };
            utilMock.Setup(u => u.IsValidReferralCode(req.ReferralCode)).Returns(true);
            referralMock.Setup(r => r.GetReferral(req.ReferralCode, req.Name)).ReturnsAsync(new Referral(defaultUid, "Jose", ReferralMethod.EMAIL, "A1B2C3"));

            var result = await controller.GetReferral(req);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var referral = Assert.IsType<Referral>(ok.Value);
            Assert.Equal("Jose", referral.Name);
        }

        // GetReferral check invalid code response
        [Fact]
        public async Task GetReferral_InvalidCode_ReturnsBadRequest()
        {
            var req = new GetReferralRequest { ReferralCode = "XYZ", Name = "Jose" };
            utilMock.Setup(u => u.IsValidReferralCode(req.ReferralCode)).Returns(false);

            var result = await controller.GetReferral(req);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        /** tests for statistics endpoints */
        [Fact]
        public async Task GetReferralStats_ValidUid_ReturnsStats()
        {
            var expectedStats = new ReferralStatistics
            {
                Uid = defaultUid,
                TotalSent = 5,
                TotalCompleted = 3,
            };

            referralMock.Setup(r => r.GetReferralStatistics(defaultUid)).ReturnsAsync(expectedStats);

            var result = await controller.GetReferralStats(defaultUid);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var stats = Assert.IsType<ReferralStatistics>(ok.Value);

            Assert.Equal(defaultUid, stats.Uid);
            Assert.Equal(5, stats.TotalSent);
            Assert.Equal(3, stats.TotalCompleted);
            Assert.Equal(2, stats.TotalPending);
        }

        [Fact]
        public async Task GetReferralStats_EmptyUid_ReturnsDefaultStats()
        {
            referralMock.Setup(r => r.GetReferralStatistics(defaultUid)).ReturnsAsync(new ReferralStatistics());

            var result = await controller.GetReferralStats(defaultUid);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var stats = Assert.IsType<ReferralStatistics>(ok.Value);

            Assert.Equal("", stats.Uid);
            Assert.Equal(0, stats.TotalSent);
            Assert.Equal(0, stats.TotalCompleted);
            Assert.Equal(0, stats.TotalPending);
        }

        [Fact]
        public async Task GetReferralStats_NullUid_ReturnsDefaultStats()
        {
            referralMock.Setup(r => r.GetReferralStatistics("")).ReturnsAsync(new ReferralStatistics());

            var result = await controller.GetReferralStats("");
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var stats = Assert.IsType<ReferralStatistics>(ok.Value);

            Assert.Equal(0, stats.TotalSent);
            Assert.Equal(0, stats.TotalCompleted);
            Assert.Equal(0, stats.TotalPending);
        }

        [Fact]
        public async Task GetReferralStats_NoReferrals_ReturnsZeroStats()
        {
            referralMock.Setup(r => r.GetReferralStatistics(defaultUid)).ReturnsAsync(new ReferralStatistics
            {
                Uid = defaultUid,
                TotalSent = 0,
                TotalCompleted = 0
            });

            var result = await controller.GetReferralStats(defaultUid);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var stats = Assert.IsType<ReferralStatistics>(ok.Value);

            Assert.Equal(defaultUid, stats.Uid);
            Assert.Equal(0, stats.TotalSent);
            Assert.Equal(0, stats.TotalCompleted);
            Assert.Equal(0, stats.TotalPending);
        }

        [Fact]
        public async Task GetReferralStats_AllCompleted_ReturnsZeroPending()
        {
            referralMock.Setup(r => r.GetReferralStatistics(defaultUid)).ReturnsAsync(new ReferralStatistics
            {
                Uid = defaultUid,
                TotalSent = 4,
                TotalCompleted = 4
            });

            var result = await controller.GetReferralStats(defaultUid);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var stats = Assert.IsType<ReferralStatistics>(ok.Value);

            Assert.Equal(0, stats.TotalPending);
        }

    }
}
