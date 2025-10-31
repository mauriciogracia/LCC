using LCC.Interfaces;
using LCC.Models;
using System.Text.RegularExpressions;

namespace LCC.Services
{
    public class ReferralService : IReferralFeatures
    {
        //TODO MGG - In real life there should be a UserService
        List<UserPartial> users = new List<UserPartial>();
        Dictionary<string, List<Referral>> referralsByUid = new Dictionary<string, List<Referral>>();

        /// <summary>
        /// In real life the uid must already exist, since I'm mocking the service
        /// when a UID does not exist, it will be added to simplify flow
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public string GetUserReferralCode(string uid)
        {
            string code;
            UserPartial? usr = users.Find(u => u.Uid.Equals(uid));

            if (usr == null)
            {
                usr = new UserPartial();
                usr.Uid = uid;
                usr.ReferralCode = PrepareReferralCode(uid);
                users.Add(usr); //this is an in memory implementation
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
            return referralsByUid.TryGetValue(uid, out var resp) ? resp : Enumerable.Empty<Referral>();
        }

        /// <summary>
        /// prepare a unique referral code for the same UID always (generated once, kept in UserPartial)
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        private string PrepareReferralCode(string uid)
        {
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

            //Is the first referral for the user
            if (!referralsByUid.ContainsKey(uid))
                referralsByUid[uid] = new List<Referral>();

            if (!referralsByUid[uid].Contains(referral))
            {
                referralsByUid[uid].Add(referral);
                success = true;
            }

            return success;
        }

        public string PrepareMessage(ReferralMethod method, string referralCode)
        {
            return ((method == ReferralMethod.SMS) ? "Hi! " : "Hey\n")+template+referralCode;
        }

        public Referral ? GetReferral(string referralCode)
        {
            Referral ? _ref = referralsByUid
                                .SelectMany(kvp => kvp.Value)
                                .FirstOrDefault(r => r.ReferralCode == referralCode);

            return _ref ;
        }

        string template = $@"Join me in earning cash for our school by using the Carton Caps app. It’s an easy way to make a difference. 
All you have to do is buy Carton Caps participating products (like Cheerios!) and scan your grocery receipt. 
Carton Caps are worth $.10 each and they add up fast! Twice a year, our school receives a check to help pay 
for whatever we need - equipment, supplies or experiences the kids love!

Download the Carton Caps app here: https://cartoncaps.link/abfilefa90p?referral_code=";
    }
}
