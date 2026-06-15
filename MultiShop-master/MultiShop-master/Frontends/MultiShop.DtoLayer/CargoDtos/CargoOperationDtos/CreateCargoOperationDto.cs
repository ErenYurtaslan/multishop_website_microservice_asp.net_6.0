namespace MultiShop.DtoLayer.CargoDtos.CargoOperationDtos
{
    public class CreateCargoOperationDto
    {
        public string Barcode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
    }
}
