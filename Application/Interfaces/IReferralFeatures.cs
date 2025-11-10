using Domain.Entities;

namespace Application.Interfaces
{
    public interface IReferralFeatures
    {

        Task<bool> DeleteUserReferrals(string uid);

        Task<IEnumerable<Referral>> GetUserReferrals(string uid);

        Task<bool> AddReferral(Referral referral);

        Referral? GetReferral(string referralCode, string name);

        Task<bool> UpdateReferral(string referralCode, string name, ReferralStatus newStatus);

        Task<ReferralStatistics> GetReferralStatistics(string uid);
    }
}
