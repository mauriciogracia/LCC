using LCC.Models;

namespace LCC.Interfaces
{
    public interface IReferralFeatures
    {
        string GetUserReferralCode(string uid);

        IEnumerable<Referral> GetUserReferrals(string uid);

         bool AddReferral(Referral referral);
    }
}
