namespace MultiShop.WebUI.Areas.User.Models
{
    public class UserCargoTrackingViewModel
    {
        public List<CargoTrackingCardViewModel> CargoCards { get; set; } = new();
    }

    public class CargoTrackingCardViewModel
    {
        public int CargoDetailId { get; set; }
        public int Barcode { get; set; }
        public string CargoCompanyName { get; set; } = string.Empty;
        public string SenderCustomer { get; set; } = string.Empty;
        public string ReceiverCustomer { get; set; } = string.Empty;
        public string LastStatusDescription { get; set; } = "Kayıt oluşturuldu";
        public DateTime? LastOperationDate { get; set; }
        public List<CargoOperationStepViewModel> Steps { get; set; } = new();
        public List<UserOrderProductPreviewViewModel> Products { get; set; } = new();
    }

    public class CargoOperationStepViewModel
    {
        public string Description { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
    }
}
