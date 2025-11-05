using Domain.Interfaces;
using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Application.Interfaces;

namespace Application.Services
{
    public class UtilService : IUtilFeatures
    {
        /// <summary>
        /// prepare a unique referral code for the same UID always (generated once, kept in User)
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public string PrepareReferralCode(string uid)
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

        public string PrepareMessage(ReferralMethod method, string referralCode)
        {
                return ((method == ReferralMethod.SMS) ? "Hi! " : "Hey\n") + template + referralCode;
        }

        const string template = $@"Join me in earning cash for our school by using the Carton Caps app. It’s an easy way to make a difference. 
All you have to do is buy Carton Caps participating products (like Cheerios!) and scan your grocery receipt. 
Carton Caps are worth $.10 each and they add up fast! Twice a year, our school receives a check to help pay 
for whatever we need - equipment, supplies or experiences the kids love!

Download the Carton Caps app here: https://cartoncaps.link/abfilefa90p?referral_code=";
    }
}
