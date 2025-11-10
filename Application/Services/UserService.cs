using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class UserService : IUserFeatures
    {
        private readonly IRepository<User> users;
        private readonly IUtilFeatures util;
        ILog log;

        public UserService(IRepository<User> usersRepo, ILog logger, IUtilFeatures UtilFeatures)
        {
            users = usersRepo;
            util =  UtilFeatures;
            log = logger;
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
