// Backend/Controllers/VNPayReturnController.cs
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/vnpay")]
    public class VNPayReturnController : ControllerBase
    {
        [HttpGet("return")]
        public IActionResult Return()
        {
            var query = Request.Query;
            var collection = new NameValueCollection();
            foreach (var key in query.Keys.Cast<string>())
                collection[key] = query[key]!;

            var responseCode = collection["vnp_ResponseCode"];
            var invoiceId = collection["vnp_TxnRef"];

            if (responseCode == "00")
            {
                return Redirect($"https://shop.example.com/thanh-toan/thanh-cong?invoiceId={invoiceId}");
            }

            return Redirect($"https://shop.example.com/thanh-toan/that-bai?invoiceId={invoiceId}");
        }
    }
}