using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure;
using Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tests;

namespace ReferralTests
{
    public class ReferralServiceTests
    {
        private TestSetup setup;
        
        public ReferralServiceTests()
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
            string code = await setup.userService.GetUserReferralCode(setup.defaultUid);

            Assert.True(setup.util.IsValidReferralCode(code));
        }

        [Fact]
        public async void SameReferralCodePerUser()
        {
            string code1 = await setup.userService.GetUserReferralCode(setup.defaultUid);
            string code2 = await setup.userService.GetUserReferralCode(setup.defaultUid);

            Assert.Equal(code1, code2);
            Assert.Equal(6, code1.Length);
        }

        [Fact]
        public async void UniqueReferralPerUser()
        {
            string code1 = await setup.userService.GetUserReferralCode(setup.defaultUid);
            string code2 = await setup.userService.GetUserReferralCode("uid456");
            string code3 = await setup.userService.GetUserReferralCode(setup.defaultUid);

            Assert.NotEqual(code1, code2);
            Assert.Equal(code1, code3);
        }

        [Fact]
        public async void EmptyListingReferrals()
        {
            IEnumerable<Referral> refs = await setup.referrals.GetUserReferrals("INVALID");

            Assert.Empty(refs);
        }

        private async void AddReferralAssert(ReferralMethod method, int expectedLength, bool expectedToAdd = true )
        {
            bool success = await setup.referrals.AddReferral(new Referral(setup.defaultUid, setup.referralName, method, setup.refCode));

            if (expectedToAdd)
            {
                Assert.True(success);
            }
            else
            {
                Assert.False(success);
            }

            IEnumerable<Referral> refs = await setup.referrals.GetUserReferrals(setup.defaultUid);
            var len = refs.Count();

            Assert.True(len == expectedLength);
        }
        [Fact]
        public async void AddingAndListingReferrals()
        {
            await setup.referrals.DeleteUserReferrals(setup.defaultUid);
            IEnumerable<Referral> refs = await setup.referrals.GetUserReferrals(setup.defaultUid);

            Assert.Empty(refs);

            AddReferralAssert(ReferralMethod.SMS, 1);
            AddReferralAssert(ReferralMethod.EMAIL, 2);
            AddReferralAssert(ReferralMethod.SHARE, 3);

            //adding the same referral should NOT really add it
            AddReferralAssert(ReferralMethod.EMAIL, 3, false);
        }

        [Fact]
        public void VeryInviteMessageSMS()
        {
            string msg = setup.util.PrepareMessage(ReferralMethod.SMS, setup.refCode);

            Assert.True(msg[..4] == "Hi! ");
        }

        [Fact]
        public void VeryInviteMessageOtherApps()
        {
            string msg = setup.util.PrepareMessage(ReferralMethod.SHARE, setup.refCode);

            Assert.True(msg[..4] == "Hey\n");
        }

        [Fact]
        public async void ReferredPersonWorks()
        {

            Referral refA = new Referral(setup.defaultUid,  setup.referralName, ReferralMethod.SMS, setup.refCode);
            bool success = await setup.referrals.AddReferral(refA);
            Assert.True(success);
            Referral? refB = setup.referrals.GetReferral(setup.refCode,  setup.referralName);
            Assert.Equal(refA, refB);
        }

        [Fact]
        public async void NonReferredPersonWorks()
        {
            await setup.referrals.DeleteUserReferrals(setup.defaultUid);
            Referral? refB = setup.referrals.GetReferral(setup.refCode,  setup.referralName);

            Assert.True(refB == null);
        }

        [Fact]
        public async void UpdatingReferralWorks()
        {
            Referral refA = new Referral(setup.defaultUid,  setup.referralName, ReferralMethod.SMS, setup.refCode);
            bool success = await setup.referrals.AddReferral(refA);

            success = await setup.referrals.UpdateReferral(refA.ReferralCode,  setup.referralName, ReferralStatus.Started);
            Assert.True(success);
            Referral? refB = setup.referrals.GetReferral(setup.refCode,  setup.referralName);

            Assert.NotNull(refB);
            Assert.True(refB.Status == ReferralStatus.Started);

            success = await setup.referrals.UpdateReferral(refA.ReferralCode,  setup.referralName, ReferralStatus.Completed);
            Assert.True(success);
            Referral? refC = setup.referrals.GetReferral(setup.refCode,  setup.referralName);

            Assert.NotNull(refC);
            Assert.True(refC.Status == ReferralStatus.Completed);
        }
    }
}
