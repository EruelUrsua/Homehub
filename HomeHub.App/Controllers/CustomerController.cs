using AutoMapper;
using HomeHub.App.Models;
using HomeHub.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using static System.Formats.Asn1.AsnWriter;
using System.Xml.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Azure.Core;
using HomeHub.App.Services;
using System.Linq;

namespace HomeHub.App.Controllers
{
    public class CustomerController : Controller
    {

        private readonly HomeHubContext context;
        private readonly PayMayaService _payMayaService;

        public CustomerController(HomeHubContext context)
        {
            this.context = context;
            _payMayaService = new PayMayaService();
        }

        public IActionResult Index(int promoIndex = 0)
        {
            var ongoingPromos = context.Promos.Where(p => p.PromoEnd > DateTime.Now).
                OrderBy(p => p.PromoEnd).ToList();

            if (ongoingPromos.Count == 0)
            {
                return View(); // If no promos return empty view
            }

            // Ensure the promo index is within the bounds of available promos
            promoIndex = promoIndex >= ongoingPromos.Count ? 0 : promoIndex;

            ViewBag.Promos = ongoingPromos;
            ViewBag.CurrentPromoIndex = promoIndex;

            List<Business> businesses = context.Businesses.ToList();
            ViewBag.Businesses = businesses;

            var model = new CHomeViewModel
            {
                ProductProviders = context.Businesses.Where(x => x.Businesstype == '0').ToList(),
                ServiceProviders = context.Businesses.Where(x => x.Businesstype == '1').ToList()
            };

            return View(model);
        }

        public IActionResult OrderProduct()
        {
            var categories = context.Businesses
                .Where(b => b.Businesstype == '0')
                .Select(b => b.OfferList)
                .Distinct()
                .ToList();

            ViewBag.Categories = categories;

            //To only show Product Providers
            List<Business> list = context.Businesses.Where(x => x.Businesstype == '0').ToList();
            ViewBag.Businesses = list;
            return View(list);
        }

        public IActionResult AvailService()
        {
            // Get the unique categories from the OfferList in the Businesses table
            var categories = context.Businesses
                .Where(b => b.Businesstype == '1')
                .Select(b => b.OfferList)
                .Distinct()
                .ToList();

            ViewBag.Categories = categories;

            //To only show Service Providers
            List<Business> list = context.Businesses.Where(x => x.Businesstype == '1').ToList();
            ViewBag.Businesses = list; // Store businesses in ViewBag
            return View(list);
        }


        public IActionResult OrderListProduct(int businessId)
        {
            var provider = context.Businesses.FirstOrDefault(x => x.UserID == businessId);

            var products = context.Products.Where(x => x.ProviderID == businessId).ToList();

            ViewBag.ProviderID = businessId;
            ViewBag.BusinessName = provider.BusinessName;
            ViewBag.Address = provider.CompanyAddress;

            return View(products);
        }

        public IActionResult AvailListService(int businessId)
        {
            var provider = context.Businesses.FirstOrDefault(x => x.UserID == businessId);

            var services = context.Services.Where(x => x.ProviderID == businessId).ToList();

            ViewBag.ProviderID = businessId;
            ViewBag.BusinessName = provider.BusinessName;
            ViewBag.Address = provider.CompanyAddress;

            return View(services);
        }

        [HttpGet]
        public IActionResult ConfirmOrder()
        {
            return View(new OrderAvailViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(OrderAvailViewModel model, int businessId)
        {

            if (string.IsNullOrWhiteSpace(model.ddeliv))
            {
                ModelState.AddModelError("ddeliv", "Delivery date is required.");
            }

            if (string.IsNullOrWhiteSpace(model.tdeliv))
            {
                ModelState.AddModelError("tdeliv", "Delivery time is required.");
            }


            if (model.qty == 0)
            {
                model.qty = 1;
            }

            decimal TotalPrice = Convert.ToDecimal(model.price) * model.qty;
            decimal Discount = 0;

            if (!string.IsNullOrWhiteSpace(model.promo))
            {
                var promo = GetPromo(model.promo);
                if (promo != null)
                {
                    TotalPrice -= TotalPrice * promo.Discount;
                    Discount = promo.Discount;
                }
            }
            
            else if (string.IsNullOrWhiteSpace(model.promo))
            {
                model.promo = "No Promo Used";
            }
                    
            var userId = "9cfade9d-8909-4419-8ec6-5f7acc6071c3"; //Will replace with logged-in user id retrieval logic || input simular userID to a customer userID
            var user = await context.Customers.FindAsync(userId);

            if (user == null)
            {
                // Handle user not found scenario
                return NotFound();
            }

            ClientOrder entity = new ClientOrder();
            entity.BusinessId = businessId.ToString();
            entity.OrderDate = DateTime.Parse(model.ddeliv);
            entity.Schedule = DateTime.Parse(model.tdeliv);
            entity.OrderedPs = model.chosen;
            entity.Fee = TotalPrice;
            entity.PromoCode = model.promo;
            entity.UserId = "9cfade9d-8909-4419-8ec6-5f7acc6071c3"; //input similar userID to customer userID
            entity.FirstName = user.Firstname;
            entity.LastName = user.Lastname;
            //entity.UserId = int.Parse(model.userID);
            entity.ReportId = 1; //remove
            entity.RatingId = 1; //remove
            entity.Quantity = model.qty;
            entity.ModeOfPayment = model.mode;

            var maxRatingId = await context.ClientOrders.MaxAsync(o => (int?)o.RatingId) ?? 0;
            entity.RatingId = maxRatingId + 1;

            await context.AddAsync(entity);
            await context.SaveChangesAsync();


            model.discount = Discount;
            model.totalPrice = TotalPrice;

            return View("OrderSummary", model);
        }

        //For discounts
        private Promo GetPromo(string code)
        {
            return context.Promos.FirstOrDefault(p => p.PromoCode == code && p.PromoEnd > DateTime.Now);
        }

        public IActionResult UserProfile()
        {
            return View();
        }

        public IActionResult ViewOrders()
        {
            //Retrieve all OrdersLogs entries for the logged-in user (dagdagan ng UserID na filter pag ayos na)
            var orders = context.OrdersLogs.ToList();

            //Retrieve all rated order IDs to identify which orders are rated
            var ratedOrderIds = context.Ratings.Select(r => r.OrderId).ToHashSet();

            //Attach an "IsRated" flag to each OrdersLog entry based on whether it exists in the Ratings table
            foreach (var order in orders)
            {
                // Check if the order ID is in the set of rated order IDs
                order.IsRated = ratedOrderIds.Contains(int.Parse(order.OrderId));
            }

            return View(orders);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> ShowEligibleOrdersForRefund(int refundId, string refundReason)
        {
            // Fetch delivered orders from the OrdersLog (assuming "Delivered" status)
            var deliveredOrders = await context.OrdersLogs
                .Where(log => log.Status == "Delivered" && log.OrderId != null)
                .ToListAsync();

            // Get the list of product providers (businesses with BusinessType = 0)
            var eligibleUserIds = await context.Businesses
                .Where(b => b.Businesstype == '0')
                .Select(b => b.UserID) 
                .ToListAsync();

            var refundList = new List<RefundRequest>();

            foreach (var orderLog in deliveredOrders)
            {
                // Ensure BusinessId (string) is converted to integer for comparison with UserId
                int businessId;
                if (!int.TryParse(orderLog.BusinessId, out businessId))
                {
                    continue;
                }

                // Skip if the order is not from a business with BusinessType = 0
                if (!eligibleUserIds.Contains(businessId)) continue;

                // Check if there's an existing refund request for the order
                bool refundExists = await context.RefundRequests
                    .AnyAsync(r => r.OrderId == orderLog.OrderId);

                if (refundExists) continue;

                // Check if the order was delivered within the last 7 days
                var daysSinceDelivery = (DateTime.Now - orderLog.Date).TotalDays;

                if (daysSinceDelivery > 7) continue;  // Skip if more than 7 days have passed

                // Prepare refund request data
                var refundRequest = new RefundRequest
                {
                    RefundId = refundId,
                    OrderId = orderLog.OrderId,
                    // Skip adding ClientId here since it's nullable
                    BusinessId = orderLog.BusinessId,
                    Item = orderLog.Item,
                    RefundQuantity = orderLog.Qty,
                    RefundStatus = "Pending", // Default to Pending when creating the request
                    RefundRequestDate = DateTime.Now,
                    Fee = orderLog.Fee,
                    PromoCode = orderLog.PromoCode,
                    RefundReason = refundReason,
                    RefundAmount = 0 
                };

                refundList.Add(refundRequest);
            }

            // Check if refundList is empty and add a message
            if (refundList.Count == 0)
            {
                ViewBag.NoEligibleRefunds = "No eligible refund requests found.";
            }

            return View(refundList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRefund(string orderId, string refundReason)
        {
            // Fetch the order log entry for the given OrderId
            var orderLog = await context.OrdersLogs
                .FirstOrDefaultAsync(log => log.OrderId == orderId);

            if (orderLog == null)
            {
                // Set an error message in TempData for display on the next page
                TempData["ErrorMessage"] = "The specified order does not exist or has been removed.";

                return RedirectToAction("ShowEligibleOrdersForRefund");
            }

            // Check if a refund request already exists for this order
            bool refundExists = await context.RefundRequests
                .AnyAsync(r => r.OrderId == orderId);

            if (refundExists)
            {
                TempData["ErrorMessage"] = "A refund request has already been submitted for this order.";

                return RedirectToAction("ShowEligibleOrdersForRefund");
            }

            // Create a new RefundRequest based on the submitted form data
            var refundRequest = new RefundRequest
            {
                OrderId = orderId,
                BusinessId = orderLog.BusinessId, // Fetch the BusinessId from OrdersLog
                Item = orderLog.Item,
                RefundQuantity = orderLog.Qty,
                Fee = orderLog.Fee,
                RefundReason = refundReason,
                RefundStatus = "Pending", // Default to Pending
                PromoCode= orderLog.PromoCode,  
                RefundRequestDate = DateTime.Now,
                RefundAmount = 0 
            };

            // Add the new refund request to the database
            context.RefundRequests.Add(refundRequest);
            await context.SaveChangesAsync();

            // Redirect back to the list of eligible refunds
            return RedirectToAction("ShowEligibleOrdersForRefund");
        }

        [HttpPost]
        public IActionResult RateProvider(string LogId)
        {
            var orderLog = context.OrdersLogs.FirstOrDefault(o => o.LogId == LogId);

            if (orderLog == null)
            {
                TempData["ErrorMessage"] = "Order not found for the given Log ID.";
                return RedirectToAction("ViewOrders");
            }

            if (int.TryParse(orderLog.OrderId, out int clientId))
            {
                var clientOrder = context.ClientOrders.FirstOrDefault(co => co.ClientId == clientId);

                if (clientOrder == null)
                {
                    TempData["ErrorMessage"] = "No corresponding client order found.";
                    return RedirectToAction("ViewOrders");
                }

                var business = context.Businesses.FirstOrDefault(b => b.UserID == int.Parse(clientOrder.BusinessId));
                if (business != null)
                {
                    ViewBag.BusinessName = business.BusinessName;
                }

                return View("RateProvider", clientOrder);
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid Client ID format.";
                return RedirectToAction("ViewOrders");
            }
        }

        [HttpPost]
        public IActionResult SubmitRating(int score, string comments, int clientId, int businessId)
        {

            if (string.IsNullOrWhiteSpace(comments))
            {
                TempData["ErrorMessage"] = "Please enter your feedback.";
                return RedirectToAction("RateProvider", new { LogId = clientId.ToString() });
            }

            var rating = new Rating
            {
                OrderId = clientId,
                Score = score,
                Comments = comments,
                BusinessId = businessId,
                Date = DateTime.Now
            };

            context.Ratings.Add(rating);
            context.SaveChanges();

            return RedirectToAction("RatingConfirmation");
        }

        public IActionResult RatingConfirmation()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ReportProblem(string LogId)
        {
            var orderLog = context.OrdersLogs.FirstOrDefault(o => o.LogId == LogId);

            if (orderLog == null)
            {
                TempData["ErrorMessage"] = "Order not found for the given Log ID.";
                return RedirectToAction("ViewOrders");
            }

            if (int.TryParse(orderLog.OrderId, out int clientId))
            {
                var clientOrder = context.ClientOrders.FirstOrDefault(co => co.ClientId == clientId);

                if (clientOrder == null)
                {
                    TempData["ErrorMessage"] = "No corresponding client order found.";
                    return RedirectToAction("ViewOrders");
                }

                var business = context.Businesses.FirstOrDefault(b => b.UserID == int.Parse(clientOrder.BusinessId));
                if (business != null)
                {
                    ViewBag.BusinessName = business.BusinessName;
                }

                return View("ReportProblem", clientOrder);
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid Client ID format.";
                return RedirectToAction("ViewOrders");
            }
        }

        [HttpPost]
        public IActionResult SubmitReport(string title, string description, int clientId)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["ErrorMessage"] = "Please enter a title.";
                return RedirectToAction("ReportProblem", new { LogId = clientId.ToString() });
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                TempData["ErrorMessage"] = "Please Describe the problem.";
                return RedirectToAction("ReportProblem", new { LogId = clientId.ToString() });
            }

            var report = new Report
            {
                ReportId = GenerateReportId(),
                Title = title,
                Description = description,
                Date = DateTime.Now.Date,
                UserId = clientId.ToString()
            };

            context.Reports.Add(report);
            context.SaveChanges();

            return RedirectToAction("ReportConfirmation");
        }

        public IActionResult ReportConfirmation()
        {
            return View();
        }


        private string GenerateReportId()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd");

            //Retrieve lastest Report created today
            var lastReport = context.Reports.Where(r => r.ReportId.StartsWith("RPT-" + datePart))
                                            .OrderByDescending(r => r.ReportId).FirstOrDefault();

            //Make 1 default if no reports today exist
            int sequenceNumber = 1;

            //Increment Sequence Number
            if (lastReport != null)
            {
                string[] parts = lastReport.ReportId.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2].Trim(), out int lastNumber))
                {
                    sequenceNumber = lastNumber + 1;
                }
            }

            return $"RPT-{datePart}-{sequenceNumber:D3}";
        }

        [HttpPost]
        public async Task<ActionResult> PayOnline(string LogId)
        {
            var orderLog = context.OrdersLogs.FirstOrDefault(o => o.LogId == LogId);

            if (orderLog == null)
            {
                TempData["ErrorMessage"] = "Order not found for the given Log ID.";
                return RedirectToAction("ViewOrders");
            }

            decimal amount = orderLog.Fee;
            string orderRef = "ORDER-123456"; // Unique order ID

            string qrCodeUrl = await _payMayaService.PayOnline(amount, orderRef);

            if (!string.IsNullOrEmpty(qrCodeUrl))
            {
                ViewBag.QRCodeUrl = qrCodeUrl;
                return View(); 
            }

            return View("Error");
        }
    }
}