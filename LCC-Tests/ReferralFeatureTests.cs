using LCC.Interfaces;
using LCC.Models;
using LCC.Services;

namespace LCC.Tests
{
    public class ReferralFeatureTests
    {
        IReferralFeatures _ref = new ReferralService();
        string defaultUid = "uid123";
        string refCode = "A1B2C3";

        [Fact]
        public void SameReferralCodePerUser()
        {
            var code1 = _ref.GetUserReferralCode(defaultUid);
            var code2 = _ref.GetUserReferralCode(defaultUid);

            Assert.Equal(code1, code2);
            Assert.Equal(6, code1.Length);
        }

        [Fact]
        public void UniqueReferralPerUser()
        {
            var code1 = _ref.GetUserReferralCode(defaultUid);
            var code2 = _ref.GetUserReferralCode("uid456");
            var code3 = _ref.GetUserReferralCode(defaultUid);

            Assert.NotEqual(code1, code2);
            Assert.Equal(code1, code3);
        }

        [Fact]
        public void AddingListingReferrals()
        {
            bool success;
            IEnumerable<Referral> refs = _ref.GetUserReferrals(defaultUid);

            Assert.Empty(refs);

            success = _ref.AddReferral(new Referral(defaultUid, "Jose", Models.ReferralMethod.SMS, refCode));
            refs = _ref.GetUserReferrals(defaultUid);
            Assert.True(refs.Count() == 1);

            success = _ref.AddReferral(new Referral(defaultUid, "Jose", Models.ReferralMethod.EMAIL, refCode));
            refs = _ref.GetUserReferrals(defaultUid);
            Assert.True(refs.Count() == 2);

            success = _ref.AddReferral(new Referral(defaultUid, "Jose", Models.ReferralMethod.SHARE, refCode));
            refs = _ref.GetUserReferrals(defaultUid);
            Assert.True(refs.Count() == 3);


            //adding thes same referral should NOT added
            success = _ref.AddReferral(new Referral(defaultUid, "Jose", Models.ReferralMethod.EMAIL, refCode));
            Assert.False(success);

            refs = _ref.GetUserReferrals(defaultUid);
            Assert.True(refs.Count() == 3); //the number keeps the same 3 items
        }

        [Fact]
        public void VeryInviteMessageSMS()
        {
            string msg = _ref.PrepareMessage(ReferralMethod.SMS, refCode);

            Assert.True(msg.Substring(0, 4) == "Hi! ");
        }

        [Fact]
        public void VeryInviteMessageOtherApps()
        {
            string msg = _ref.PrepareMessage(ReferralMethod.SHARE, refCode);

            Assert.True(msg.Substring(0, 4) == "Hey\n");
        }

        [Fact]
        public void ReferredPersonWorks()
        {
            Referral refA = new Referral(defaultUid, "Jose", ReferralMethod.SMS, refCode);
            _ref.AddReferral(refA);

            Referral ?refB = _ref.GetReferral(refCode);

            Assert.Equal(refA, refB);
            
        }

        [Fact]
        public void NonReferredPersonWorks()
        {
            Referral? refB = _ref.GetReferral(refCode);

            Assert.True(refB == null);
        }
    }
}
