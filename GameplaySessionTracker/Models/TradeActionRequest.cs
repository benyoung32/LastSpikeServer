namespace GameplaySessionTracker.Models
{
    public class TradeActionRequest
    {
        public Guid PlayerId { get; set; }
        public bool Accept { get; set; }
    }
}
