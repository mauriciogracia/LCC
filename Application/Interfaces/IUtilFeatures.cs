
using Domain;

namespace Application.Interfaces
{
    public interface IUtilFeatures
    {
        string PrepareReferralCode(string uid);
        bool IsValidReferralCode(string code);

        string PrepareMessage(ReferralMethod method, string referralCode);

    }
}
