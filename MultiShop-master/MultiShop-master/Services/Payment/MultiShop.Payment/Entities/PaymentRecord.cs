namespace MultiShop.Payment.Entities
{
    public class PaymentRecord
    {
        public int PaymentRecordId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string CardLast4 { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public string Status { get; set; } = "Success";
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
        public string? OrderSummary { get; set; }
    }
}
