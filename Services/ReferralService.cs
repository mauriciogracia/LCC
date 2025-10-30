using LCC.Interfaces;
using LCC.Models;

namespace LCC.Services
{
    public class ReferralService : IReferralFeatures
    {
        List<UserPartial> users = new List<UserPartial>();

        /// <summary>
        /// In real life the uid must already exist, since I'm mocking the service
        /// when a UID does not exist, it will be added to simplify flow
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public string GetUserReferralCode(string uid)
        {
            string code;
            UserPartial ?usr = users.Find(u => u.uid.Equals(uid));

            if(usr == null)
            {
                usr = new UserPartial();
                usr.uid = uid;
                usr.referralCode = PrepareReferralCode(uid);
                users.Add(usr); //this is an in memory implementation
            }

            code = usr.referralCode ;
            return code;
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
