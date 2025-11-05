using Application.Interfaces;
using Application.Services;
using Domain;
using Infrastructure;

namespace Tests
{
    public class ReferralFeatureTests
    {
        readonly ILog _log ;
        IReferralFeatures refService ;
        string defaultUid = "uid123";
        string refCode = "A1B2C3";
        string referralName = "Jose";

        public ReferralFeatureTests()
        {
            _log = new ConsoleLogger();
            refService = new ReferralService(_log);
        }

        [Fact]
        public async void DetectIncorrectReferralCodes()
        {
            Assert.False(await refService.IsValidReferralCode(""));
            Assert.False(await refService.IsValidReferralCode("AA"));
            Assert.False(await refService.IsValidReferralCode("ASDFASDFASDF"));
            Assert.False(await refService.IsValidReferralCode("%&/()"));
            Assert.False(await refService.IsValidReferralCode("12345!")); 
        }

        [Fact]
        public async void ValidateReferralCode()
        {
            string code = await refService.GetUserReferralCode(defaultUid);

            Assert.True(await refService.IsValidReferralCode(code));
        }

        [Fact]
        public async void SameReferralCodePerUser()
        {
            string code1 = await refService.GetUserReferralCode(defaultUid);
            string code2 = await refService.GetUserReferralCode(defaultUid);

            Assert.Equal(code1, code2);
            Assert.Equal(6, code1.Length);
        }

        [Fact]
        public async void UniqueReferralPerUser()
        {
            string code1 = await refService.GetUserReferralCode(defaultUid);
            string code2 = await refService.GetUserReferralCode("uid456");
            string code3 = await refService.GetUserReferralCode(defaultUid);

            Assert.NotEqual(code1, code2);
            Assert.Equal(code1, code3);
        }

        [Fact]
        public async void EmptyListingReferrals()
        {
            IEnumerable<Referral> refs = await refService.GetUserReferrals("INVALID");

            Assert.Empty(refs);
        }
            [Fact]
        public async void AddingAndListingReferrals()
        {
            bool success;
            
            refService.ClearUserReferrals(defaultUid);
            IEnumerable<Referral> refs = await refService.GetUserReferrals(defaultUid);

            Assert.Empty(refs);

            success = await refService.AddReferral(new Referral(defaultUid, referralName, ReferralMethod.SMS, refCode));
            refs = await refService.GetUserReferrals(defaultUid);
            Assert.True(refs.Count() == 1);

            success = await refService.AddReferral(new Referral(defaultUid, referralName, ReferralMethod.EMAIL, refCode));
            refs = await refService.GetUserReferrals(defaultUid);
            Assert.True(refs.Count() == 2);

            success = await refService.AddReferral(new Referral(defaultUid, referralName, ReferralMethod.SHARE, refCode));
            refs = await refService.GetUserReferrals(defaultUid);
            Assert.True(refs.Count() == 3);


            //adding the same referral should NOT really add it
            success = await refService.AddReferral(new Referral(defaultUid, referralName, ReferralMethod.EMAIL, refCode));
            Assert.False(success);

            refs = await refService.GetUserReferrals(defaultUid);
            Assert.True(refs.Count() == 3); //the number keeps the same 3 items
        }

        [Fact]
        public async void VeryInviteMessageSMS()
        {
            string msg = await refService.PrepareMessage(ReferralMethod.SMS, refCode);

            Assert.True(msg.Substring(0, 4) == "Hi! ");
        }

        [Fact]
        public async void VeryInviteMessageOtherApps()
        {
            string msg = await refService.PrepareMessage(ReferralMethod.SHARE, refCode);

            Assert.True(msg.Substring(0, 4) == "Hey\n");
        }

        [Fact]
        public async void ReferredPersonWorks()
        {

            Referral refA = new Referral(defaultUid, referralName, ReferralMethod.SMS, refCode);
            bool success = await refService.AddReferral(refA);

            Referral ?refB = await refService.GetReferral(refCode, referralName);
            Assert.Equal(refA, refB);
        }

        [Fact]
        public async void NonReferredPersonWorks()
        {
            refService.ClearUserReferrals(defaultUid);
            Referral? refB = await refService.GetReferral(refCode, referralName);

            Assert.True(refB == null);
        }

        [Fact]
        public async void UpdatingReferralWorks()
        {
            Referral refA = new Referral(defaultUid, referralName, ReferralMethod.SMS, refCode);
            bool success = await refService.AddReferral(refA);

            success  = await refService.UpdateReferral(refA.ReferralCode, referralName, ReferralStatus.STARTED);
            Referral? refB = await refService.GetReferral(refCode, referralName);

            Assert.NotNull(refB);
            Assert.True(refB.Status == ReferralStatus.STARTED);

            success = await refService.UpdateReferral(refA.ReferralCode, referralName, ReferralStatus.COMPLETED);
            Referral? refC = await refService.GetReferral(refCode, referralName);

            Assert.NotNull(refC);
            Assert.True(refC.Status == ReferralStatus.COMPLETED);
        }
    }
}
