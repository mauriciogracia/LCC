namespace LCC.Models
{
    public class PrepareMessageRequest
    {
        public ReferralMethod Method { get; set; }
        public string ReferralCode { get; set; } = string.Empty;
    }
}
