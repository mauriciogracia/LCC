using LCC.Models;

namespace LCC.Interfaces
{
    public interface IReferralFeatures
    {
        string GetUserReferralCode(string uid);

        bool IsValidReferralCode(string code);
        IEnumerable<Referral> GetUserReferrals(string uid);

        string PrepareMessage(ReferralMethod method, string referralCode);

        bool AddReferral(Referral referral);

        Referral ? GetReferral(string referralCode);
    }
}
