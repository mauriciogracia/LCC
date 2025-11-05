namespace Domain
{
    /// <summary>
    /// The real user would have all the fields: login, created date, email, etc
    /// 
    /// For the given scope I only need a "key-value" like this one
    /// </summary>
    public class UserPartial
    {
        public string Uid { get; set; } = string.Empty;
        public string ReferralCode { get; set; } = string.Empty;
    }
}
