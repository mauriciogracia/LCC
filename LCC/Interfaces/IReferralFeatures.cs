using LCC.Models;

namespace LCC.Interfaces
{
    public interface IReferralFeatures
    {
        Task<string> GetUserReferralCode(string uid);

        Task<bool> IsValidReferralCode(string code);
        
        Task<IEnumerable<Referral>> GetUserReferrals(string uid);

        Task<string> PrepareMessage(ReferralMethod method, string referralCode);

        Task<bool> AddReferral(Referral referral);

        Task<Referral?> GetReferral(string referralCode);

        Task<bool> UpdateReferral(string referralCode, ReferralStatus newStatus);
    }
}
