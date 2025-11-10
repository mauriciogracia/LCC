using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserFeatures
    {
        Task<string> GetUserReferralCode(string uid);

        Task<bool> AttributeReferral(string referralCode, string refereeUid);
    }
}
