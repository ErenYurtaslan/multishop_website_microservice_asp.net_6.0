using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Cargo.BusinessLayer.Abstract;
using MultiShop.Cargo.DtoLayer.Dtos.CargoOperationDtos;
using MultiShop.Cargo.EntityLayer.Concrete;

namespace MultiShop.Cargo.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CargoOperationsController : ControllerBase
    {
        private readonly ICargoOperationService _CargoOperationService;
        private readonly ICargoDetailService _cargoDetailService;
        public CargoOperationsController(
            ICargoOperationService CargoOperationService,
            ICargoDetailService cargoDetailService)
        {
            _CargoOperationService = CargoOperationService;
            _cargoDetailService = cargoDetailService;
        }

        [HttpGet]
        public IActionResult CargoOperationList()
        {
            var values = _CargoOperationService.TGetAll();
            return Ok(values);
        }

        [HttpPost]
        public IActionResult CreateCargoOperation(CreateCargoOperationDto createCargoOperationDto)
        {
            if (!HasValidCargoDetailForBarcode(createCargoOperationDto.Barcode))
            {
                return BadRequest("Geçerli bir kargo barkoduna bağlı operasyon oluşturulmalıdır.");
            }

            CargoOperation CargoOperation = new CargoOperation()
            {
                Barcode = createCargoOperationDto.Barcode,
                Description = createCargoOperationDto.Description,
                OperationDate = createCargoOperationDto.OperationDate
            };
            _CargoOperationService.TInsert(CargoOperation);
            return Ok("Kargo İşlemi Başarıyla Oluşturuldu");
        }

        [HttpDelete]
        public IActionResult RemoveCargoOperation(int id)
        {
            _CargoOperationService.TDelete(id);
            return Ok("Kargo İşlemi Başarıyla Silindi");
        }

        [HttpGet("{id}")]
        public IActionResult GetCargoOperationById(int id)
        {
            var values = _CargoOperationService.TGetById(id);
            return Ok(values);
        }

        [HttpPut]
        public IActionResult UpdateCargoOperation(UpdateCargoOperationDto updateCargoOperationDto)
        {
            if (!HasValidCargoDetailForBarcode(updateCargoOperationDto.Barcode))
            {
                return BadRequest("Geçerli bir kargo barkoduna bağlı operasyon güncellenmelidir.");
            }

            CargoOperation CargoOperation = new CargoOperation()
            {
                Barcode = updateCargoOperationDto.Barcode,
                CargoOperationId = updateCargoOperationDto.CargoOperationId,
                Description = updateCargoOperationDto.Description,
                OperationDate = updateCargoOperationDto.OperationDate
            };
            _CargoOperationService.TUpdate(CargoOperation);
            return Ok("Kargo İşlemi Başarıyla Güncellendi");
        }

        private bool HasValidCargoDetailForBarcode(string? barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return false;
            if (!int.TryParse(barcode.Trim(), out var parsedBarcode)) return false;
            return _cargoDetailService.TGetAll().Any(x => x.Barcode == parsedBarcode);
        }
    }
}
