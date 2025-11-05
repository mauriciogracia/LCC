
using Domain;

namespace Application.Interfaces
{
    public interface IReferralFeatures
    {
        Task<string> GetUserReferralCode(string uid);

        Task<bool> IsValidReferralCode(string code);


        void ClearUserReferrals(string uid);

        Task<IEnumerable<Referral>> GetUserReferrals(string uid);

        Task<string> PrepareMessage(ReferralMethod method, string referralCode);

        Task<bool> AddReferral(Referral referral);

        Task<Referral?> GetReferral(string referralCode, string name);

        Task<bool> UpdateReferral(string referralCode, string name, ReferralStatus newStatus);
    }
}
