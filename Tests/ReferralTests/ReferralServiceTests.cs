using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ReferralTests
{
    public class ReferralServiceTests
    {
        private readonly ILog _log;
        private readonly UserService users;
        private readonly ReferralService referrals;
        private readonly IUtilFeatures util;
        private readonly string defaultUid = "U1";
        private readonly string refCode = "A1B2C3";
        private readonly string referralName = "Jose";

        public ReferralServiceTests()
        {
            _log = new ConsoleLogger();

            var options = new DbContextOptionsBuilder<ReferralDbContext>()
            .UseInMemoryDatabase(databaseName: "TestReferralDb")
            .Options;

            var dbContext = new ReferralDbContext(options);

            dbContext.Database.EnsureCreated();

            
            var referralRepo = new ReferralRepository(dbContext);
            util = new UtilService();


            
            referrals = new ReferralService(referralRepo, _log, util);
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
        public async void EmptyListingReferrals()
        {
            IEnumerable<Referral> refs = await referrals.GetUserReferrals("INVALID");

            Assert.Empty(refs);
        }
        [Fact]
        public async void AddingAndListingReferrals()
        {
            bool success;

            await referrals.DeleteUserReferrals(defaultUid);
            IEnumerable<Referral> refs = await referrals.GetUserReferrals(defaultUid);

            Assert.Empty(refs);

            success = await referrals.AddReferral(new Referral(defaultUid, referralName, ReferralMethod.SMS, refCode));
            Assert.True(success);
            refs = await referrals.GetUserReferrals(defaultUid);
            Assert.True(refs.Count() == 1);

            success = await referrals.AddReferral(new Referral(defaultUid, referralName, ReferralMethod.EMAIL, refCode));
            Assert.True(success);
            refs = await referrals.GetUserReferrals(defaultUid);
            Assert.True(refs.Count() == 2);

            success = await referrals.AddReferral(new Referral(defaultUid, referralName, ReferralMethod.SHARE, refCode));
            Assert.True(success);
            refs = await referrals.GetUserReferrals(defaultUid);
            Assert.True(refs.Count() == 3);


            //adding the same referral should NOT really add it
            success = await referrals.AddReferral(new Referral(defaultUid, referralName, ReferralMethod.EMAIL, refCode));
            Assert.False(success);
            refs = await referrals.GetUserReferrals(defaultUid);
            Assert.True(refs.Count() == 3); //the number keeps the same 3 items
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

        [Fact]
        public async void ReferredPersonWorks()
        {

            Referral refA = new Referral(defaultUid, referralName, ReferralMethod.SMS, refCode);
            bool success = await referrals.AddReferral(refA);
            Assert.True(success);
            Referral? refB = referrals.GetReferral(refCode, referralName);
            Assert.Equal(refA, refB);
        }

        [Fact]
        public async void NonReferredPersonWorks()
        {
            await referrals.DeleteUserReferrals(defaultUid);
            Referral? refB = referrals.GetReferral(refCode, referralName);

            Assert.True(refB == null);
        }

        [Fact]
        public async void UpdatingReferralWorks()
        {
            Referral refA = new Referral(defaultUid, referralName, ReferralMethod.SMS, refCode);
            bool success = await referrals.AddReferral(refA);

            success = await referrals.UpdateReferral(refA.ReferralCode, referralName, ReferralStatus.Started);
            Assert.True(success);
            Referral? refB = referrals.GetReferral(refCode, referralName);

            Assert.NotNull(refB);
            Assert.True(refB.Status == ReferralStatus.Started);

            success = await referrals.UpdateReferral(refA.ReferralCode, referralName, ReferralStatus.Completed);
            Assert.True(success);
            Referral? refC = referrals.GetReferral(refCode, referralName);

            Assert.NotNull(refC);
            Assert.True(refC.Status == ReferralStatus.Completed);
        }
    }
}
