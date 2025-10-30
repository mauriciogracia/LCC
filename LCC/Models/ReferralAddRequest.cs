namespace LCC.Models
{
    public class ReferralAddRequest
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public ReferralMethod Method { get; set; }
    }
}
