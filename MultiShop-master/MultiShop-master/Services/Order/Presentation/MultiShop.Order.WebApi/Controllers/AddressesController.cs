using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MultiShop.Order.Application.Features.CQRS.Commands.AddressCommands;
using MultiShop.Order.Application.Features.CQRS.Handlers.AddressHandlers;
using MultiShop.Order.Application.Features.CQRS.Queries.AddressQueries;
using MultiShop.Order.Application.Interfaces;
using MultiShop.Order.Domain.Entities;

namespace MultiShop.Order.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly GetAddressQueryHandler _getAddressQueryHandler;
        private readonly GetAddressByIdQueryHandler _getAddressByIdQueryHandler;
        private readonly CreateAddressCommandHandler _createAddressCommandHandler;
        private readonly UpdateAddressCommandHandler _updateAddressCommandHandler;
        private readonly RemoveAddressCommandHandler _removeAddressCommandHandler;
        private readonly IRepository<Address> _addressRepository;
        public AddressesController(GetAddressQueryHandler getAddressQueryHandler, GetAddressByIdQueryHandler getAddressByIdQueryHandler, CreateAddressCommandHandler createAddressCommandHandler, UpdateAddressCommandHandler updateAddressCommandHandler, RemoveAddressCommandHandler removeAddressCommandHandler, IRepository<Address> addressRepository)
        {
            _getAddressQueryHandler = getAddressQueryHandler;
            _getAddressByIdQueryHandler = getAddressByIdQueryHandler;
            _createAddressCommandHandler = createAddressCommandHandler;
            _updateAddressCommandHandler = updateAddressCommandHandler;
            _removeAddressCommandHandler = removeAddressCommandHandler;
            _addressRepository = addressRepository;
        }

        [HttpGet]
        public async Task<IActionResult> AddressList()
        {
            var values = await _getAddressQueryHandler.Handle();
            return Ok(values);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddressById(int id)
        {
            var values = await _getAddressByIdQueryHandler.Handle(new GetAddressByIdQuery(id));
            return Ok(values);
        }

        [HttpGet("ByEmailDistinct")]
        public async Task<IActionResult> GetDistinctAddressesByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Ok(new List<object>());
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var all = await _addressRepository.GetAllAsync();

            var values = all
                .Where(x => !string.IsNullOrWhiteSpace(x.Email) && x.Email.Trim().ToLowerInvariant() == normalizedEmail)
                .GroupBy(x => new
                {
                    Email = (x.Email ?? string.Empty).Trim().ToLowerInvariant(),
                    Detail1 = (x.Detail1 ?? string.Empty).Trim().ToLowerInvariant(),
                    Detail2 = (x.Detail2 ?? string.Empty).Trim().ToLowerInvariant()
                })
                .Select(g => g.OrderByDescending(a => a.AddressId).First())
                .OrderByDescending(x => x.AddressId)
                .Select(x => new
                {
                    x.AddressId,
                    x.UserId,
                    x.Name,
                    x.Surname,
                    x.Email,
                    x.Phone,
                    x.Country,
                    x.District,
                    x.City,
                    x.Detail1,
                    x.Detail2,
                    x.Description,
                    x.ZipCode
                })
                .ToList();

            return Ok(values);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress(CreateAddressCommand command)
        {
            await _createAddressCommandHandler.Handle(command);
            return Ok("Adres bilgisi başarıyla eklendi");
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAddress(UpdateAddressCommand command)
        {
            await _updateAddressCommandHandler.Handle(command);
            return Ok("Adres bilgisi başarıyla güncellendi");
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveAddress(int id)
        {
            await _removeAddressCommandHandler.Handle(new RemoveAddressCommand(id));
            return Ok("Adres başarıyla silindi");
        }
    }
}
