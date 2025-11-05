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

        public ReferralsControllerTests()
        {
            controller = new ReferralsController(referralMock.Object, utilMock.Object, logMock.Object);
        }

        // GetReferralCode: valid uid
        [Fact]
        public async Task GetReferralCode_ValidUid_ReturnsCode()
        {
            string uid = "U1";
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
            referralMock.Setup(r => r.GetUserReferrals("U1")).ReturnsAsync([new("U1", "Jose", ReferralMethod.EMAIL, "A1B2C3")]);

            var result = await controller.GetReferrals("U1");
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<Referral>>(ok.Value);
            Assert.Single(list);
        }

        //  GetReferrals validate a null uid
        [Fact]
        public async Task GetReferrals_NullUid_ReturnsBadRequest()
        {
            var result = await controller.GetReferrals(null);
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        // AddReferral validate when correct request
        [Fact]
        public async Task AddReferral_ValidRequest_ReturnsTrue()
        {
            var req = new ReferralAddRequest { Uid = "U1", Name = "Jose", Method = ReferralMethod.EMAIL, ReferralCode = "A1B2C3" };
            utilMock.Setup(u => u.IsValidReferralCode(req.ReferralCode)).Returns(true);
            referralMock.Setup(r => r.AddReferral(It.IsAny<Referral>())).ReturnsAsync(true);

            var result = await controller.AddReferral(req);
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.True((bool)ok.Value);
        }

        // AddReferral validate invalid referral code
        [Fact]
        public async Task AddReferral_InvalidCode_ReturnsBadRequest()
        {
            var req = new ReferralAddRequest { Uid = "U1", Name = "Jose", Method = ReferralMethod.EMAIL, ReferralCode = "XYZ" };
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
            Assert.True((bool)ok.Value);
        }

        // GetReferral check valid referral
        [Fact]
        public async Task GetReferral_Valid_ReturnsReferral()
        {
            var req = new GetReferralRequest { ReferralCode = "A1B2C3", Name = "Jose" };
            utilMock.Setup(u => u.IsValidReferralCode(req.ReferralCode)).Returns(true);
            referralMock.Setup(r => r.GetReferral(req.ReferralCode, req.Name)).ReturnsAsync(new Referral("U1", "Jose", ReferralMethod.EMAIL, "A1B2C3"));

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
    }
}
