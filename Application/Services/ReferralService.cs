using Application.Interfaces;
using Domain;
using System.Text.RegularExpressions;

namespace Application.Services
{
    public class ReferralService : IReferralFeatures
    {
        static readonly List<User> _users = new List<User>();
        static readonly Dictionary<string, List<Referral>> referralsByUserId = new Dictionary<string, List<Referral>>();
        ILog log;

        public ReferralService(ILog logger)
        {
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
            string code;
            User? usr = _users.Find(u => u.Uid.Equals(uid));

            //a user is created on the fly to simplify scenarios and no need for seed data
            if (usr == null)
            {
                usr = new User();
                usr.Uid = uid;
                usr.ReferralCode = await PrepareReferralCode(uid);
                _users.Add(usr); 
                log.info($"user created: {uid}, with refCode: {usr.ReferralCode}");
            }
            else
            {
                log.info($"user exists with refCode: {usr.ReferralCode}");
            }

            code = usr.ReferralCode;
            return code;
        }

        /// <summary>
        /// List user referrals 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Referral>> GetUserReferrals(string uid)
        {
            log.info($"Getting referrals for : {uid}");
            
            return await Task.Run(() =>
            {
                return referralsByUserId.TryGetValue(uid, out var resp) ? resp : Enumerable.Empty<Referral>();
            });
        }

        public void ClearUserReferrals(string uid)
        {
            if (referralsByUserId.TryGetValue(uid, out var referrals))
            {
                referrals.Clear(); // Clears the list in-place
            }
        }
        /// <summary>
        /// prepare a unique referral code for the same UID always (generated once, kept in User)
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        private async Task<string> PrepareReferralCode(string uid)
        {
            log.info($"Generating referral code : {uid}");

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(uid));
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return await Task.Run(() =>
            {
                return string.Concat(hash.Take(6).Select(b => chars[b % chars.Length]));
            });
        }

        public async Task<bool> IsValidReferralCode(string code)
        {
            return await Task.Run(() =>
            {
                return Regex.IsMatch(code, @"^[A-Z0-9]{6}$");
            });
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

            return await Task.Run(() =>
            {
                log.info($"Adding referral...{uid}");

                //Is the first referral for the user
                if (!referralsByUserId.ContainsKey(uid))
                    referralsByUserId[uid] = new List<Referral>();

                if (!referralsByUserId[uid].Contains(referral))
                {
                    referralsByUserId[uid].Add(referral);
                    success = true;
                }
                else
                {
                    log.error($"Referral could not be added");
                }

                return success;
            });
        }
        public async Task<string> PrepareMessage(ReferralMethod method, string referralCode)
        {
            return await Task.Run(() =>
            {
                log.info($"Preparing message based on method/app");
                return ((method == ReferralMethod.SMS) ? "Hi! " : "Hey\n") + template + referralCode;
            });
        }

        public async Task<bool> UpdateReferral(string referralCode, string name, ReferralStatus newStatus)
        {
            bool success = false;
            Referral ?referral = await GetReferral(referralCode, name);

            return await Task.Run(() =>
            {
                if (referral == null)
                {
                    log.error($"No referral found for {referralCode}");
                }
                else
                {
                    referral.Status = newStatus;
                    referral.UpdatedAt = DateTime.Now;

                    referralsByUserId[referral.Uid].Find(r => r.Uid == referralCode);
                    success = true;
                }

                return success;
            });
        }

        public async Task<Referral?> GetReferral(string referralCode, string name)
        {
            return await Task.Run(() =>
            {
                Referral? referral = referralsByUserId
                                .SelectMany(kvp => kvp.Value)
                                .FirstOrDefault(r => r.ReferralCode == referralCode && r.Name == name);

                return referral;
            });
        }

        //TODO MGG - this should be in a .json or properties file instead or retrieved from a template repository

        const string template = $@"Join me in earning cash for our school by using the Carton Caps app. It’s an easy way to make a difference. 
All you have to do is buy Carton Caps participating products (like Cheerios!) and scan your grocery receipt. 
Carton Caps are worth $.10 each and they add up fast! Twice a year, our school receives a check to help pay 
for whatever we need - equipment, supplies or experiences the kids love!

Download the Carton Caps app here: https://cartoncaps.link/abfilefa90p?referral_code=";
    }
}
