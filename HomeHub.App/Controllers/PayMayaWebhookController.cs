using HomeHub.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.App.Controllers
{
    [ApiController]
    [Route("api/paymaya/webhook")]
    public class PayMayaWebhookController : ControllerBase
    {
        private readonly HomeHubContext _context;

        public PayMayaWebhookController(HomeHubContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromBody] PayMayaWebhookPayload payload)
        {
            if (payload == null || string.IsNullOrEmpty(payload.Id) || string.IsNullOrEmpty(payload.Status))
            {
                return BadRequest("Invalid payload");
            }

            //Find the order in OrdersLog using OrderId
            var orderLog = await _context.OrdersLogs.FirstOrDefaultAsync(o => o.LogId == payload.RequestReferenceNumber);

            if (orderLog == null)
            {
                return NotFound("Order not found");
            }

            //Update payment status based on webhook event
            switch (payload.Status)
            {
                case "PAYMENT_SUCCESS":
                    orderLog.PayStatus = "Paid";
                    break;
                case "PAYMENT_FAILED":
                    orderLog.PayStatus = "Failed";
                    break;
                case "PAYMENT_EXPIRED":
                    orderLog.PayStatus = "Expired";
                    break;
            }

            await _context.SaveChangesAsync();

            //return Ok();
            return Ok(new { message = "Payment status updated successfully" });
        }

        public class PayMayaWebhookPayload
        {
            public string Id { get; set; }
            public string Status { get; set; }
            public string RequestReferenceNumber { get; set; }
        }
    }
}
