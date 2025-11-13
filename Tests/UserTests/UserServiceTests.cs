using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Infrastructure.Repository;
using Tests;

namespace UserTests
{

    public class UserServiceTests
    {
        
        private readonly TestSetup setup;
        private readonly string defaultUid = "U1";
        private readonly string refCode = "A1B2C3";

        public UserServiceTests()
        {
            setup = new TestSetup();   
        }

      

        [Fact]
        public void DetectIncorrectReferralCodes()
        {
            Assert.False(setup.util.IsValidReferralCode(""));
            Assert.False(setup.util.IsValidReferralCode("AA"));
            Assert.False(setup.util.IsValidReferralCode("ASDFASDFASDF"));
            Assert.False(setup.util.IsValidReferralCode("%&/()"));
            Assert.False(setup.util.IsValidReferralCode("12345!"));
        }

        [Fact]
        public async void ValidateReferralCode()
        {
            string code = await setup.userService.GetUserReferralCode(defaultUid);

            Assert.True(setup.util.IsValidReferralCode(code));
        }

        [Fact]
        public async void SameReferralCodePerUser()
        {
            string code1 = await setup.userService.GetUserReferralCode(defaultUid);
            string code2 = await setup.userService.GetUserReferralCode(defaultUid);

            Assert.Equal(code1, code2);
            Assert.Equal(6, code1.Length);
        }

        [Fact]
        public async void UniqueReferralPerUser()
        {
            string code1 = await setup.userService.GetUserReferralCode(defaultUid);
            string code2 = await setup.userService.GetUserReferralCode("uid456");
            string code3 = await setup.userService.GetUserReferralCode(defaultUid);

            Assert.NotEqual(code1, code2);
            Assert.Equal(code1, code3);
        }

        [Fact]
        public void VeryInviteMessageSMS()
        {
            string msg = setup.util.PrepareMessage(ReferralMethod.SMS, refCode);

            Assert.True(msg[..4] == "Hi! ");
        }

        [Fact]
        public void VeryInviteMessageOtherApps()
        {
            string msg = setup.util.PrepareMessage(ReferralMethod.SHARE, refCode);

            Assert.True(msg[..4] == "Hey\n");
        }
    }
}
