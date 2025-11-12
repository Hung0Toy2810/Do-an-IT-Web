// Backend/Controllers/VNPayIPNController.cs
using Backend.Service.Payment;
using Backend.Service.Checkout;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/vnpay")]
    public class VNPayIPNController : ControllerBase
    {
        private readonly IVNPayService _vnpayService;
        private readonly ICheckoutService _checkoutService;

        public VNPayIPNController(IVNPayService vnpayService, ICheckoutService checkoutService)
        {
            _vnpayService = vnpayService ?? throw new ArgumentNullException(nameof(vnpayService));
            _checkoutService = checkoutService ?? throw new ArgumentNullException(nameof(checkoutService));
        }

        [HttpGet("ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> IPN()
        {
            var query = Request.Query;
            var collection = new NameValueCollection();
            foreach (var key in query.Keys.Cast<string>())
                collection[key] = query[key]!;

            var ipnResult = _vnpayService.ProcessPaymentIPN(collection);
            if (ipnResult.RspCode != "00")
            {
                return Ok(new { RspCode = "97", Message = "Invalid signature" });
            }

            if (long.TryParse(collection["vnp_TxnRef"], out var invoiceId))
            {
                await _checkoutService.HandleVNPayIPNSuccessAsync(invoiceId);
            }

            return Ok(new { RspCode = "00", Message = "Confirm Success" });
        }
    }
}