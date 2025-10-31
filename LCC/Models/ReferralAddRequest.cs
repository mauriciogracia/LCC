namespace LCC.Models
{
    public class ReferralAddRequest
    {
        public string Uid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ReferralMethod Method { get; set; }
        public string ReferralCode { get; set; } = string.Empty;

        public string validate()
        {
            string resp = string.Empty;

            if(string.IsNullOrEmpty(Uid))
            {
                return FieldCanNotBeEmpty("Uid");
            }
            if (string.IsNullOrEmpty(Name))
            {
                return FieldCanNotBeEmpty("Name"); 
            }
            if (string.IsNullOrEmpty(ReferralCode))
            {
                return FieldCanNotBeEmpty("ReferralCode");
            }

            return resp;
        }
        private string FieldCanNotBeEmpty(string field)
        {
            return $"{field} cannot be null or empty";
        }
    }
    public class PrepareMessageRequest
    {
        public ReferralMethod Method { get; set; }
        public string ReferralCode { get; set; } = string.Empty;
    }
}
