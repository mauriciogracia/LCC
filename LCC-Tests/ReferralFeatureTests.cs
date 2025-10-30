using LCC.Interfaces;
using LCC.Services;

namespace LCC.Tests
{
    public class ReferralFeatureTests
    {

        IReferralFeatures _ref = new ReferralService();
        [Fact]
        public void SameReferralCodePerUser()
        {
            var code1 = _ref.GetUserReferralCode("uid123");
            var code2 = _ref.GetUserReferralCode("uid123");

            Assert.Equal(code1, code2); 
            Assert.Equal(6, code1.Length);
        }

        [Fact]
        public void UniqueReferralPerUser()
        {
            var code1 = _ref.GetUserReferralCode("uid123");
            var code2 = _ref.GetUserReferralCode("uid456");
            var code3 = _ref.GetUserReferralCode("uid123");

            Assert.NotEqual(code1, code2);
            Assert.Equal(code1, code3);
        }
    }
}
