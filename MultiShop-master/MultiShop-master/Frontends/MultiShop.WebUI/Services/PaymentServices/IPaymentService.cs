namespace MultiShop.WebUI.Services.PaymentServices
{
    public class CreatePaymentDto
    {
        public decimal TotalAmount { get; set; }
        public string CardLast4 { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public string? OrderSummary { get; set; }
    }

    public class ResultPaymentDto
    {
        public int PaymentRecordId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string CardLast4 { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime PaidAt { get; set; }
        public string? OrderSummary { get; set; }
    }

    public interface IPaymentService
    {
        Task<ResultPaymentDto?> CreatePaymentAsync(CreatePaymentDto dto);
    }
}
