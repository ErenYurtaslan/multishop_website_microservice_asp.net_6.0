using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiShop.Payment.Context;
using MultiShop.Payment.Dtos;
using MultiShop.Payment.Entities;
using System.Security.Claims;

namespace MultiShop.Payment.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly PaymentContext _context;

        public PaymentsController(PaymentContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { error = "Kullanıcı bilgisi alınamadı." });
            }

            if (dto == null || dto.TotalAmount <= 0)
            {
                return BadRequest(new { error = "Geçersiz ödeme tutarı." });
            }

            // Maskeleme: kart numarasının yalnızca son 4 hanesini saklıyoruz, gerisi tutulmuyor.
            var last4 = (dto.CardLast4 ?? string.Empty).Trim();
            if (last4.Length > 4) last4 = last4[^4..];

            var record = new PaymentRecord
            {
                UserId = userId,
                TotalAmount = dto.TotalAmount,
                CardLast4 = last4,
                CardHolderName = dto.CardHolderName ?? string.Empty,
                OrderSummary = dto.OrderSummary,
                Status = "Success",
                PaidAt = DateTime.UtcNow
            };

            _context.PaymentRecords.Add(record);
            await _context.SaveChangesAsync();

            return Ok(new ResultPaymentDto
            {
                PaymentRecordId = record.PaymentRecordId,
                UserId = record.UserId,
                TotalAmount = record.TotalAmount,
                CardLast4 = record.CardLast4,
                CardHolderName = record.CardHolderName,
                Status = record.Status,
                PaidAt = record.PaidAt,
                OrderSummary = record.OrderSummary
            });
        }

        [HttpGet("MyPayments")]
        public async Task<IActionResult> MyPayments()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var list = await _context.PaymentRecords
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaidAt)
                .Select(p => new ResultPaymentDto
                {
                    PaymentRecordId = p.PaymentRecordId,
                    UserId = p.UserId,
                    TotalAmount = p.TotalAmount,
                    CardLast4 = p.CardLast4,
                    CardHolderName = p.CardHolderName,
                    Status = p.Status,
                    PaidAt = p.PaidAt,
                    OrderSummary = p.OrderSummary
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var list = await _context.PaymentRecords
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
            return Ok(list);
        }
    }
}
