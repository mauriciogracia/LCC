namespace Application.Interfaces
{
    public interface IUserFeatures
    {
        Task<string> AuthenticateUser(string username, string password);

        Task<string> GetUserReferralCode(string uid);

        Task<bool> AttributeReferral(string referralCode, string refereeUid);
    }
}
