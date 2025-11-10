using Domain.Entities;

namespace Application.DTO
{
    public class ReferralAddRequest
    {
        public string Uid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ReferralMethod Method { get; set; }
        public string ReferralCode { get; set; } = string.Empty;
    }
}
