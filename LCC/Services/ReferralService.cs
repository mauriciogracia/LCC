using LCC.Interfaces;
using LCC.Models;
using System.Text.RegularExpressions;

namespace LCC.Services
{
    public class ReferralService : IReferralFeatures
    {
        readonly List<UserPartial> _users = new List<UserPartial>();
        readonly Dictionary<string, List<Referral>> _referralsByUid = new Dictionary<string, List<Referral>>();
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
        public string GetUserReferralCode(string uid)
        {
            string code;
            UserPartial? usr = _users.Find(u => u.Uid.Equals(uid));

            //a user is created on the fly to simplify scenarios and no need for seed data
            if (usr == null)
            {
                usr = new UserPartial();
                usr.Uid = uid;
                usr.ReferralCode = PrepareReferralCode(uid);
                _users.Add(usr); //this is an in memory implementation
                log.info($"user created: {uid}, with refCode: {usr.ReferralCode}");
            }
            else
            {
                log.info($"user already exists with refCode: {usr.ReferralCode}");
            }

            code = usr.ReferralCode;
            return code;
        }

        /// <summary>
        /// List user referrals 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public IEnumerable<Referral> GetUserReferrals(string uid)
        {
            log.info($"Getting referrals for : {uid}");
            return _referralsByUid.TryGetValue(uid, out var resp) ? resp : Enumerable.Empty<Referral>();
        }

        /// <summary>
        /// prepare a unique referral code for the same UID always (generated once, kept in UserPartial)
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        private string PrepareReferralCode(string uid)
        {
            log.info($"Generating referral code : {uid}");

            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(uid));
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return string.Concat(hash.Take(6).Select(b => chars[b % chars.Length]));
        }

        public bool IsValidReferralCode(string code)
        {
            return Regex.IsMatch(code, @"^[A-Z0-9]{6}$");
        }
        /// <summary>
        /// Add a referral to the uid of the referral
        /// </summary>
        /// <param name="referral"></param>
        /// <returns></returns>
        public bool AddReferral(Referral referral)
        {
            bool success = false;
            string uid = referral.Uid;

            log.info($"Adding referral...");
            //Is the first referral for the user
            if (!_referralsByUid.ContainsKey(uid))
                _referralsByUid[uid] = new List<Referral>();

            if (!_referralsByUid[uid].Contains(referral))
            {
                _referralsByUid[uid].Add(referral);
                success = true;
            }
            else
            {
                log.error($"Referral could not be added");
            }

            return success;
        }

        public string PrepareMessage(ReferralMethod method, string referralCode)
        {
            log.info($"Preparing message based on method/app");
            return ((method == ReferralMethod.SMS) ? "Hi! " : "Hey\n")+template+referralCode;
        }

        public Referral ? GetReferral(string referralCode)
        {
            Referral ? _ref = _referralsByUid
                                .SelectMany(kvp => kvp.Value)
                                .FirstOrDefault(r => r.ReferralCode == referralCode);

            return _ref ;
        }

        const string template = $@"Join me in earning cash for our school by using the Carton Caps app. It’s an easy way to make a difference. 
All you have to do is buy Carton Caps participating products (like Cheerios!) and scan your grocery receipt. 
Carton Caps are worth $.10 each and they add up fast! Twice a year, our school receives a check to help pay 
for whatever we need - equipment, supplies or experiences the kids love!

Download the Carton Caps app here: https://cartoncaps.link/abfilefa90p?referral_code=";
    }
}
