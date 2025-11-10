namespace Domain.Entities
{
    public class ReferralStatistics
    {
        public string Uid { get; set; } = string.Empty;
        public int TotalSent { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalPending => TotalSent - TotalCompleted;
    }
}
