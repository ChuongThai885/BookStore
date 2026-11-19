namespace Cart.Application.Contract
{
    public class CartTouchedEvent
    {
        public Guid UserId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
