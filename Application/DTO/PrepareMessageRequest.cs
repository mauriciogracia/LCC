using Domain.Entities;

namespace Application.DTO
{
    public class PrepareMessageRequest
    {
        public ReferralMethod Method { get; set; }
        public string ReferralCode { get; set; } = string.Empty;
    }
}
