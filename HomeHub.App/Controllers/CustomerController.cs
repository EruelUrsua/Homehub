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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net;
using static HomeHub.App.Models.PayMayaVM;

namespace HomeHub.App.Controllers
{
    public class CustomerController : Controller
    {

        private readonly HomeHubContext context;
        private readonly PayMayaService _payMayaService;
        private readonly UserManager<ApplicationUser> userManager;

        public CustomerController(HomeHubContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            _payMayaService = new PayMayaService();
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<string> GetCurrentUserId()
        {
            ApplicationUser usr = await GetCurrentUserAsync();
            return usr?.Id;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => userManager.GetUserAsync(HttpContext.User);
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
            string getCurrentUserId = await GetCurrentUserId();

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
                    
            var userId = getCurrentUserId; //"47ae60c1-5de0-4f86-9a6a-5ce24df3b2c0"; //Will replace with logged-in user id retrieval logic || input simular userID to a customer userID
            var user = await context.ApplicationUsers.FindAsync(userId);

            if (user == null)
            {
                // Handle user not found scenario
                return NotFound();
            }

            ClientOrder entity = new ClientOrder();
            entity.BusinessId = businessId;
            entity.OrderDate = DateTime.Parse(model.ddeliv);
            entity.Schedule = DateTime.Parse(model.tdeliv);
            entity.OrderedPs = model.chosen;
            entity.Fee = TotalPrice;
            entity.PromoCode = model.promo;
            entity.UserId = getCurrentUserId; //input similar userID to customer userID
            entity.FirstName = user.Firstname;
            entity.LastName = user.Lastname;
            //entity.UserId = int.Parse(model.userID);
            entity.ReportId = 1; //remove
            entity.RatingId = 1; //remove
            entity.Quantity = model.qty;
            entity.ModeOfPayment = model.mode;
            entity.Status = "Pending";

            var maxRatingId = await context.ClientOrders.MaxAsync(o => (int?)o.RatingId) ?? 0;
            entity.RatingId = maxRatingId + 1;

            await context.AddAsync(entity);
            await context.SaveChangesAsync();

            // **Create a notification for the provider**
            var notification = new Notification
            {
                BusinessId = businessId,
                Message = $"A new order has been placed by {user.Firstname} {user.Lastname}.",
                IsRead = false,
            };

            context.Notifications.Add(notification);
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

        public async Task<ActionResult> UserProfile()
        {
            ApplicationUser user = await GetCurrentUserAsync();
            if (user == null)
                return View("Index");

            var model = new UserProfileVM
            {
                FirstName = user.Firstname,
                LastName = user.Lastname,
                Address = user.Address
            };

            return View(model);
        }

        public async Task<IActionResult> EditAddress()
        {
            ApplicationUser user = await GetCurrentUserAsync();
            if (user == null)
                return View("Index");

            var model = new UpdateAddressVM
            {
                Address = user.Address
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAddress(UpdateAddressVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            ApplicationUser user = await GetCurrentUserAsync();
            if (user == null)
                return View("Index");

            user.Address = model.Address;
            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Address updated successfully!";
                return RedirectToAction("UserProfile");
            }


            return View(model);
        }

        public IActionResult ViewOrders()
        {
            string userId = userManager.GetUserId(User);
            //Retrieve all OrdersLogs entries for the logged-in user
            //var orders = context.OrdersLogs.ToList();
            var orders = context.OrdersLogs
                .Where(o => o.UserId == userId)
                .ToList();

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
            // Get the logged-in user's ID
            string userId = await GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                ViewBag.ErrorMessage = "You need to be logged in to view eligible orders for a refund.";
                return View(new List<RefundRequest>()); 
            }

            // Fetch delivered orders from the OrdersLog (assuming "Delivered" status)
            var deliveredOrders = await context.OrdersLogs
                .Where(log => log.Status == "Delivered" && log.OrderId != null)
                .ToListAsync();

            var eligibleUserIds = await context.Businesses
                .Where(b => b.Businesstype == '0')
                .Select(b => b.UserID) 
                .ToListAsync();

            var refundList = new List<RefundRequest>();

            foreach (var orderLog in deliveredOrders)
            {
                int businessId = orderLog.BusinessId; 

                // Skip if the order is not from a business with BusinessType = 0
                if (!eligibleUserIds.Contains(businessId)) continue;

                // Check if there's an existing refund request for the order
                bool refundExists = await context.RefundRequests
                    .AnyAsync(r => r.OrderId == orderLog.OrderId);

                if (refundExists) continue;

                // Check if the order was delivered within the last 7 days
                var daysSinceDelivery = (DateTime.Now - orderLog.Date).TotalDays;

                if (daysSinceDelivery > 7) continue;  // Skip if more than 7 days have passed

                var refundRequest = new RefundRequest
                {
                    RefundId = refundId,
                    OrderId = orderLog.OrderId,
                    UserId = userId,
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
                ViewBag.NoEligibleRefunds = "Only orders delivered within the last 7 days are eligible for a refund. Orders delivered more than 7 days ago are not eligible.";
            }

            return View(refundList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestRefund(string orderId, string refundReason)
        {
            string userId = await GetCurrentUserId(); // Get the logged-in user's ID

            // Fetch the order log entry for the given OrderId and ensure it belongs to the logged-in user
            var orderLog = await context.OrdersLogs
                .FirstOrDefaultAsync(log => log.OrderId == orderId && log.UserId == userId);

            if (orderLog == null)
            {
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
                UserId = userId,
                BusinessId = orderLog.BusinessId, 
                Item = orderLog.Item,
                RefundQuantity = orderLog.Qty,
                Fee = orderLog.Fee,
                RefundReason = refundReason,
                RefundStatus = "Pending", 
                PromoCode= orderLog.PromoCode,  
                RefundRequestDate = DateTime.Now,
                RefundAmount = 0 
            };

            context.RefundRequests.Add(refundRequest);

            // Update the order status in OrdersLog to track the refund request
            orderLog.Status = "Refund Requested";

            // Create a notification for the provider
            var notification = new Notification
            {
                BusinessId = orderLog.BusinessId,
                Message = $"A new refund request has been made for order {orderId}.",
                IsRead = false,
            };

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your refund request has been submitted successfully.";

            return RedirectToAction("ShowEligibleOrdersForRefund");
        }

        public async Task<IActionResult> RefundHistory(string statusFilter)
        {
            var userId = await GetCurrentUserId(); // Get the logged-in user's ID

            if (string.IsNullOrEmpty(userId))
            {
                ViewBag.ErrorMessage = "You need to be logged in to view your refund history.";
                return View(new List<RefundRequest>()); // Return an empty list to avoid errors
            }

            var refundRequests = context.RefundRequests.Where(r => r.UserId == userId).AsQueryable();

            // Apply filter if a status is selected
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                if (statusFilter == "Approved")
                    refundRequests = refundRequests.Where(r => r.RefundStatus == "Refund Accepted");
                else if (statusFilter == "Rejected")
                    refundRequests = refundRequests.Where(r => r.RefundStatus == "Refund Rejected");
                else if (statusFilter == "Pending")
                    refundRequests = refundRequests.Where(r => r.RefundStatus == "Pending");
                else
                    refundRequests = refundRequests.Where(r => r.RefundStatus == statusFilter);
            }

            var filteredRefunds = await refundRequests.OrderByDescending(r => r.RefundRequestDate).ToListAsync();

            ViewBag.StatusFilter = statusFilter;
            return View(filteredRefunds);
        }

        public async Task<IActionResult> SellerProfile(int businessId)
        {
            var seller = await context.Businesses.FirstOrDefaultAsync(b => b.UserID == businessId);

            if (seller == null)
            {
                TempData["ErrorMessage"] = "Seller profile not found.";
                return RedirectToAction("RefundHistory");
            }

            return View(seller);
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

                var business = context.Businesses.FirstOrDefault(b => b.UserID == clientOrder.BusinessId);
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
        public async Task<IActionResult> ReportProblem(string LogId)
        {
            string userId = await GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("SignIn", "Account");
            }

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

                var business = context.Businesses.FirstOrDefault(b => b.UserID == clientOrder.BusinessId);
                if (business != null)
                {
                    ViewBag.BusinessName = business.BusinessName;
                }

                ViewBag.UserId = userId;

                return View("ReportProblem", clientOrder);
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid Client ID format.";
                return RedirectToAction("ViewOrders");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SubmitReport(string title, string description, int clientId)
        {
            string userId = await GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("SignIn", "Account");
            }

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
                UserId = userId
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
            /*
            var orderLog = context.OrdersLogs.FirstOrDefault(o => o.LogId == LogId);

            if (orderLog == null)
            {
                TempData["ErrorMessage"] = "Order not found for the given Log ID.";
                return RedirectToAction("ViewOrders");
            }

            decimal amount = orderLog.Fee;
            string orderRef = "ORDER-" + LogId; // Unique order ID
            var items = new List<>

            /*string qrCodeUrl = await _payMayaService.PayOnline(amount, orderRef);

            if (!string.IsNullOrEmpty(qrCodeUrl))
            {
                ViewBag.QRCodeUrl = qrCodeUrl;
                return View(); 
            }
            
            return View("Error");*/
            try
            {
                var orderLog = context.OrdersLogs.FirstOrDefault(o => o.LogId == LogId);


                decimal totalAmount = orderLog.Fee;
                string currency = "PHP";

                List<PayMayaVM.PayMayaItem> items = new List<PayMayaVM.PayMayaItem>
                {
                    //will change once multiple order
                    new PayMayaVM.PayMayaItem
                    {
                        name = orderLog.Item,
                        //amount = new PayMayaVM.PayMayaAmount {totalAmount = orderLog.Fee },
                        totalAmount = new PayMayaAmount {value = totalAmount, currency = currency}
                    }
                };

                string response = await _payMayaService.CreateCheckoutAsync(totalAmount, currency, items, LogId);

                return Content(response, "application/json");
            }
            catch(Exception ex)
            {
                return Content ("Error" + ex.Message);
            }
        }

        [HttpPost]
        public IActionResult CancelOrder(string LogId)
        {
            var order = context.OrdersLogs.FirstOrDefault(o => o.LogId == LogId);

            if (order != null)
            {
                int clientId;
                if (int.TryParse(order.OrderId, out clientId))
                {
                    var clientOrder = context.ClientOrders.FirstOrDefault(c => c.ClientId == clientId);

                    DateTime orderDateTime = order.OrderDate.Date + order.Date.TimeOfDay;
                    TimeSpan timeRemaining = orderDateTime - DateTime.Now;

                    if (timeRemaining.TotalHours < 24)
                    {
                        TempData["ErrorMessage"] = "Cancellation is not allowed within 24 hours of delivery.";
                        return RedirectToAction("ViewOrders");
                    }

                    if (order.Status != "Cancelled")
                    {
                        order.Status = "Cancelled";
                    }

                    if (clientOrder != null && clientOrder.Status != "Cancelled")
                    {
                        clientOrder.Status = "Cancelled";
                    }

                    context.SaveChanges();
                }
            }
            return RedirectToAction("ViewOrders");
        }

        public IActionResult ReviewRating(int LogId)
        {
            var rating = (from r in context.Ratings
                          join b in context.Businesses on r.BusinessId equals b.UserID
                          where r.OrderId == LogId
                          select new
                          {
                              r.OrderId,
                              r.Score,
                              r.Comments,
                              b.BusinessName
                          }).FirstOrDefault();

            if (rating == null)
            {
                return RedirectToAction("ViewOrders"); // Redirect if no rating exists
            }

            var model = new ReviewRatingVM
            {
                OrderId = rating.OrderId,
                Score = rating.Score,
                Comments = rating.Comments,
                BusinessName = rating.BusinessName
            };

            // Pass the rating details to the view
            return View(model);
        }

        public async Task<IActionResult> YourReports()
        {
            string userId = await GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return View("SignIn", "Account");
            }

            var userReports = context.Reports
                .Where(r => r.UserId == userId)
                .ToList();

            return View(userReports);
        }

        public IActionResult ReviewReport(string id)
        {
            var report = context.Reports.FirstOrDefault(r => r.ReportId == id);

            if (report == null)
            {
                return Index();
            }

            return View(report);
        }
    }
}