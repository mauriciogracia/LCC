using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class UserService : IUserFeatures
    {
        private readonly IRepository<User> users;
        private readonly IUtilFeatures util;
        ILog log;
        private readonly ITokenService tokenService;

        public UserService(IRepository<User> usersRepo, ILog logger, IUtilFeatures UtilFeatures, ITokenService ts)
        {
            users = usersRepo;
            util =  UtilFeatures;
            log = logger;
            tokenService = ts;
        }
        public async Task<string> AuthenticateUser(string username, string password)
        {
            var user = users.GetByFilter(u => u.Email == username).FirstOrDefault();

            if (user == null)
                return string.Empty;

            // Hash the incoming raw password
            var hashedInput = HashPassword(password);

            if (user.Password != hashedInput)
                return string.Empty;

            // Issue JWT
            return tokenService.GenerateToken(user);
        }

        private string HashPassword(string password)
        {
            // Example: PBKDF2
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Gets a UserReferral (1st the referral code is prepared and kept)
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<string> GetUserReferralCode(string uid)
        {
            string code = "";
            User? usr = await users.GetByIdAsync(uid);

            if (usr == null)
            {
                log.error($"User with {uid} does not exist");
            }
            else
            {
                //generate referral code the 1st time
                if (string.IsNullOrEmpty(usr.ReferralCode))
                {
                    usr.ReferralCode = util.PrepareReferralCode(uid);
                    await users.UpdateAsync(usr);
                }

                code = usr.ReferralCode;
                log.info($"user exists with refCode: {usr.ReferralCode}");
            }

            return code;
        }

        public async Task<bool> AttributeReferral(string referralCode, string refereeUid)
        {
            var referee = await users.GetByIdAsync(refereeUid);

            if (referee == null)
            {
                log.error($"Referee not found: {refereeUid}");
                return false;
            }

            referee.ReferralCode = referralCode;
            await users.UpdateAsync(referee);

            log.info($"Referral code {referralCode} attributed to user {refereeUid}");
            return true;
        }
    }
}
