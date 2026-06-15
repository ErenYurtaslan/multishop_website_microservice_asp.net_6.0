namespace MultiShop.DtoLayer.CargoDtos.CargoOperationDtos
{
    public class ResultCargoOperationDto
    {
        public int CargoOperationId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
    }
}
