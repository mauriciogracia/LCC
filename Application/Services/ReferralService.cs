using Application.Interfaces;
using Domain;
using Domain.Interfaces;

namespace Application.Services
{
    public class ReferralService : IReferralFeatures
    {
        private readonly IRepository<User> users;
        private readonly IRepository<Referral> referrals;
        private readonly IUtilFeatures util;
        ILog log;

        public ReferralService(IRepository<User> usersRepo, IRepository<Referral> referralsRepo, ILog logger, IUtilFeatures UtilFeatures)
        {
            users = usersRepo;
            referrals = referralsRepo;
            util = UtilFeatures;
            log = logger;
        }

        /// <summary>
        /// In real life the uid must already exist, since I'm mocking the service
        /// when a UID does not exist, it will be added to simplify flow
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
                if(string.IsNullOrEmpty(usr.ReferralCode))
                {
                    usr.ReferralCode = util.PrepareReferralCode(uid);
                    await users.UpdateAsync(usr);
                }

                code = usr.ReferralCode;
                log.info($"user exists with refCode: {usr.ReferralCode}");
            }
            
            return code;
        }
        public async Task<IEnumerable<Referral>> GetReferralsByUserIdAsync(string uid)
        {
            var allReferrals = await referrals.GetAllAsync();
            return allReferrals.Where(r => r.Uid == uid);
        }
        
        /// <summary>
        /// List user referrals 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Referral>> GetUserReferrals(string uid)
        {
            log.info($"Getting referrals for : {uid}");
            return await GetReferralsByUserIdAsync(uid);
        }

        public async Task<bool> DeleteUserReferrals(string uid)
        {
            bool success = false ;

            try
            {
                var userReferrals = await referrals.GetAllAsync();
                var toDelete = userReferrals.Where(r => r.Uid == uid).ToList();

                foreach (var referral in toDelete)
                {
                    await referrals.DeleteAsync(referral.ReferralId);
                }
                log.info("DeleteUserReferrals DONE");
                success = true;
            }catch
            {
                log.error("DeleteUserReferrals failed");
            }

            return success;
        }
       
        /// <summary>
        /// Add a referral to the uid of the referral
        /// </summary>
        /// <param name="referral"></param>
        /// <returns></returns>
        public async Task<bool> AddReferral(Referral referral)
        {
            bool success = false;
            string uid = referral.Uid;

            var refExists = await referrals.GetByFilterAsync(r => r.Name == referral.Name && r.Method == referral.Method);

            if (refExists.Any())
            {
                log.error($"Referral already exists - not added");
            }
            else
            {
                log.info($"Adding referral...{uid}");
                await referrals.AddAsync(referral);
                success = true;
            }

            return success;
        }

        public async Task<bool> UpdateReferral(string referralCode, string name, ReferralStatus newStatus)
        {
            bool success = false;
            Referral? referral = await GetReferral(referralCode, name);

                if (referral == null)
                {
                    log.error($"No referral found for {referralCode}");
                }
                else
                {
                    referral.Status = newStatus;
                    referral.UpdatedAt = DateTime.Now;
                    await referrals.UpdateAsync(referral);
                    success = true;
                }

                return success;
        }

        public async Task<Referral?> GetReferral(string referralCode, string name)
        {
            var matches = await referrals.GetByFilterAsync(r => r.ReferralCode == referralCode && r.Name == name);
            var result = matches.FirstOrDefault();
            return result;
        }

        public async Task<ReferralStatistics> GetReferralStatistics(string uid)
        {
            var userReferrals = await GetReferralsByUserIdAsync(uid);
            int completed = userReferrals.Count(r => r.Status == ReferralStatus.Completed);
            int total = userReferrals.Count();

            return new ReferralStatistics
            {
                Uid = uid,
                TotalSent = total,
                TotalCompleted = completed,
            };
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
