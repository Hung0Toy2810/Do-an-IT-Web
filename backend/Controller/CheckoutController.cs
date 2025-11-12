// Backend/Controllers/CheckoutController.cs
using Backend.Model.Entity;
using Backend.Service.Checkout;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/checkout")]
    [Authorize(Roles = "Customer")]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;

        public CheckoutController(ICheckoutService checkoutService)
        {
            _checkoutService = checkoutService ?? throw new ArgumentNullException(nameof(checkoutService));
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout([FromBody] CheckoutRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng"));

            var result = await _checkoutService.ProcessCheckoutAsync(request);

            if (!result.Success)
                return BadRequest(new { Message = result.Message });

            if (result.RequiresPayment)
            {
                return Ok(new
                {
                    Message = "Vui lòng thanh toán",
                    Data = new { result.InvoiceId, result.PaymentUrl, RequiresPayment = true }
                });
            }

            return Ok(new
            {
                Message = "Đơn hàng COD đã được tạo thành công",
                Data = new { result.InvoiceId, result.TrackingNumber }
            });
        }
    }
}