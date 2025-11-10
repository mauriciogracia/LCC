namespace Domain.Entities
{
    public class Referral : IEquatable<Referral>
    {
        public string ReferralId { get; set; }

        public string Uid { get; set; }
        public string Name { get; set; }

        public string ReferralCode { get; set; }
        public ReferralMethod Method { get; set; }

        public ReferralStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public User? User { get; set; }

        /// <summary>
        /// Creates a referral for a given user(uid), person name and method
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="name"></param>
        /// <param name="method"></param>
        public Referral(string uid, string name, ReferralMethod method, string referralCode)
        {
            ReferralId = GenerateReferralId();
            Uid = uid;
            Name = name;
            ReferralCode = referralCode;
            Method = method;
            Status = ReferralStatus.Invited;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// example ABC123XYZ 
        /// </summary>
        /// <returns></returns>
        private static string GenerateReferralId()
        {
            return string.Concat(Enumerable.Range(0, 3).Select(_ => Guid.NewGuid().ToString("N").Substring(0, 3).ToUpper()));
        }
        public bool Equals(Referral? other)
        {
            if (other is null) return false;
            return Uid == other.Uid && Name == other.Name && Method == other.Method;
        }

        public override bool Equals(object? obj) => Equals(obj as Referral);
        public override int GetHashCode() => HashCode.Combine(Name, Method);


    }
}
