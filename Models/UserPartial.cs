namespace LCC.Models
{
    /// <summary>
    /// The real user would have all the fields: login, created date, email, etc
    /// 
    /// For the given scope I only need a "key-value" like this one
    /// </summary>
    public class UserPartial
    {
        public string uid = string.Empty;
        public string referralCode = string.Empty;
    }
}
