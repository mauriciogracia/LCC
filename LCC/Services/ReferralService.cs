using LCC.Interfaces;
using LCC.Models;
using System.Collections.Generic;

namespace LCC.Services
{
    public class ReferralService : IReferralFeatures
    {
        //TODO MGG - In real life there should be a UserService
        List<UserPartial> users = new List<UserPartial>();
        //TODO MGG - create a class for this: string, List<Referral>
        Dictionary<string, List<Referral>> referralsByUid = new Dictionary<string, List<Referral>>();

        //TODO MGG - maybe pass Referral as argument and return success (true/false)
        public Referral AddReferral(string uid, string name, ReferralMethod method)
        {
            Referral referral ;

            //Is the first referral for the user
            if (!referralsByUid.ContainsKey(uid))
                referralsByUid[uid] = new List<Referral>();

            referral = new Referral(uid, name, method);

            if (!referralsByUid[uid].Contains(referral))
            {
                referralsByUid[uid].Add(referral);
            }

            return referral;
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
            UserPartial ?usr = users.Find(u => u.Uid.Equals(uid));

            if(usr == null)
            {
                usr = new UserPartial();
                usr.Uid = uid;
                usr.ReferralCode = PrepareReferralCode(uid);
                users.Add(usr); //this is an in memory implementation
            }

            code = usr.ReferralCode ;
            return code;
        }

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
    }
}
