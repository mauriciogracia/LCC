namespace Application.DTO
{
    public class ReferralUpdateRequest
    {
        public string Id { get; set; } = string.Empty;
        public string ReferralCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
