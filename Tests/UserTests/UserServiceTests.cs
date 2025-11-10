using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace UserTests
{
    public class UserServiceTests
    {
        private readonly ILog _log;
        private readonly UserService users;
        private readonly IUtilFeatures util;
        private readonly string defaultUid = "U1";
        private readonly string refCode = "A1B2C3";

        public UserServiceTests()
        {
            _log = new ConsoleLogger();

            var options = new DbContextOptionsBuilder<ReferralDbContext>()
            .UseInMemoryDatabase(databaseName: "TestReferralDb")
            .Options;

            var dbContext = new ReferralDbContext(options);

            dbContext.Database.EnsureCreated();

            var userRepo = new UserRepository(dbContext);
            var referralRepo = new ReferralRepository(dbContext);
            util = new UtilService();
            users = new UserService(userRepo, _log, util);
        }

        [Fact]
        public void DetectIncorrectReferralCodes()
        {
            Assert.False(util.IsValidReferralCode(""));
            Assert.False(util.IsValidReferralCode("AA"));
            Assert.False(util.IsValidReferralCode("ASDFASDFASDF"));
            Assert.False(util.IsValidReferralCode("%&/()"));
            Assert.False(util.IsValidReferralCode("12345!"));
        }

        [Fact]
        public async void ValidateReferralCode()
        {
            string code = await users.GetUserReferralCode(defaultUid);

            Assert.True(util.IsValidReferralCode(code));
        }

        [Fact]
        public async void SameReferralCodePerUser()
        {
            string code1 = await users.GetUserReferralCode(defaultUid);
            string code2 = await users.GetUserReferralCode(defaultUid);

            Assert.Equal(code1, code2);
            Assert.Equal(6, code1.Length);
        }

        [Fact]
        public async void UniqueReferralPerUser()
        {
            string code1 = await users.GetUserReferralCode(defaultUid);
            string code2 = await users.GetUserReferralCode("uid456");
            string code3 = await users.GetUserReferralCode(defaultUid);

            Assert.NotEqual(code1, code2);
            Assert.Equal(code1, code3);
        }

        [Fact]
        public void VeryInviteMessageSMS()
        {
            string msg = util.PrepareMessage(ReferralMethod.SMS, refCode);

            Assert.True(msg[..4] == "Hi! ");
        }

        [Fact]
        public void VeryInviteMessageOtherApps()
        {
            string msg = util.PrepareMessage(ReferralMethod.SHARE, refCode);

            Assert.True(msg[..4] == "Hey\n");
        }
    }
}
