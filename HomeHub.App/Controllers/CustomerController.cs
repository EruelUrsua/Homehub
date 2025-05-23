﻿using AutoMapper;
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
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;

namespace HomeHub.App.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerController : Controller
    {

        private readonly HomeHubContext context;
        private readonly PayMayaService _payMayaService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly EmailSenderService emailSender;

        public CustomerController(HomeHubContext context, UserManager<ApplicationUser> userManager, EmailSenderService emailSender)
        {
            this.context = context;
            _payMayaService = new PayMayaService();
            this.userManager = userManager;
            this.emailSender = emailSender;
        }

        [HttpGet]
        public async Task<string> GetCurrentUserId()
        {
            ApplicationUser usr = await GetCurrentUserAsync();
            return usr?.Id;
        }

        private Task<ApplicationUser> GetCurrentUserAsync() => userManager.GetUserAsync(HttpContext.User);

        public async Task<IActionResult> Index(int promoIndex = 0, int productPage = 1, int servicePage = 1, int pageSize = 8)
        {

            string loggedInUser = await GetCurrentUserId();
            //Recommendations 
            var recsList = context.OrdersLogs
                .Where(o => o.UserId == loggedInUser)
                .Join(context.Providers, o => o.BusinessId, p => p.UserID, (o, p) => new
                {
                    p.BusinessName,
                    p.Category,
                    p.Businesstype,
                    o.Item,
                    OrderId = Convert.ToInt32(o.OrderId)
                })
                .OrderBy(r => r.Businesstype) 
                .ToList()
                .DistinctBy(r => new { r.Item, r.BusinessName })
                .ToList();

            ViewBag.recs = recsList;

            //Featured Products
            var productsQuery = (from a in context.Providers
                                 join b in context.Products on a.UserID equals b.ProviderID
                                  join c in context.ApplicationUsers on b.ProviderID equals c.Id
                                 where a.Businesstype == false && c.IsVerified == true
                                 select new
                                 {
                                     b.ProductItem,
                                     b.Price,
                                     b.ProductImagePath,
                                     a.BusinessName,
                                     a.UserID
                                 }).ToList();

            var productList = productsQuery.ToList();
            var totalProducts = productList.Count();
            var paginatedProducts = productList.Skip((productPage - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Products = paginatedProducts;
            ViewBag.CurrentPage = productPage;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            //Featured Services
            var servicesQuery = (from a in context.Providers
                                 join b in context.Services on a.UserID equals b.ProviderID
                                 join c in context.ApplicationUsers on b.ProviderID equals c.Id
                                 where a.Businesstype == true && c.IsVerified == true
                                 select new
                                 {
                                     b.ServiceItem,
                                     b.Fee,
                                     a.BusinessName,
                                     a.UserID
                                 }).ToList();

            var totalServices = servicesQuery.Count();
            var paginatedServices = servicesQuery.Skip((servicePage - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Services = paginatedServices;
            ViewBag.CurrentServicePage = servicePage;
            ViewBag.TotalServicePages = (int)Math.Ceiling((double)totalServices / pageSize);

            //Promos
            var ongoingPromos = context.Promos.Where(p => p.PromoEnd > DateTime.Now).
                OrderBy(p => p.PromoEnd).ToList();

            if (ongoingPromos.Count == 0)
            {
                return View();
            }

            promoIndex = promoIndex >= ongoingPromos.Count ? 0 : promoIndex;

            ViewBag.Promos = ongoingPromos;
            ViewBag.CurrentPromoIndex = promoIndex;

            List<Provider> businesses = context.Providers.ToList();
            ViewBag.Businesses = context.Providers.ToList();

            var model = new CHomeViewModel
            {
                ProductProviders = context.Providers.Where(x => x.Businesstype == false).ToList(),
                ServiceProviders = context.Providers.Where(x => x.Businesstype == true).ToList()
            };

            return View(model);
        }

        public IActionResult OrderProduct()
        {
            string userId = userManager.GetUserId(User);
            var user = context.Users.FirstOrDefault(u => u.Id == userId);

            // Check if the user is under review
            if (user != null && user.IsUnderReview == true)
            {
                // Redirect to a page or show a message about the restriction
                TempData["Message"] = "Your account is under review. You cannot access this page at the moment.";
                return View("AccountUnderReview");
            }
            if (user != null && user.IsVerified == false)
            {
                // Redirect to a page or show a message about the restriction
                TempData["Message"] = "Your account is not yet verified. You cannot access this page at the moment.";
                return View("AccountForVerification");
            }
            else
            {
                var categories = context.Providers
                    .Where(b => b.Businesstype == false)
                    .Select(b => b.Category)
                    .Distinct()
                    .ToList();

                ViewBag.Categories = categories;

                var productProviders = (from p in context.Providers
                                        join u in context.ApplicationUsers on p.UserID equals u.Id
                                        where p.Businesstype == false && u.IsVerified == true
                                        select new
                                        {
                                            p.UserID,
                                            p.BusinessName,
                                            p.Category,
                                            u.PhoneNumber,
                                            u.Address,
                                            AvgPrice = context.Products
                                            .Where(pr => pr.ProviderID == p.UserID)
                                            .Average(pr => (decimal?)pr.Price) ?? 0
                                        }).ToList();

                ViewBag.Businesses = productProviders;
                return View(productProviders);
            }
        }

        public IActionResult AvailService()
        {
            string userId = userManager.GetUserId(User);
            var user = context.Users.FirstOrDefault(u => u.Id == userId);

            // Check if the user is under review
            if (user != null && user.IsUnderReview == true)
            {
                // Redirect to a page or show a message about the restriction
                TempData["Message"] = "Your account is under review. You cannot access this page at the moment.";
                return View("AccountUnderReview");
            }

            else if (user != null && user.IsVerified == false)
            {
                // Redirect to a page or show a message about the restriction
                TempData["Message"] = "Your account is not yet verified. You cannot access this page at the moment.";
                return View("AccountForVerification");
            }
            else
            {
                var categories = context.Providers
                    .Where(b => b.Businesstype == true)
                    .Select(b => b.Category)
                    .Distinct()
                    .ToList();

                ViewBag.Categories = categories;

                var serviceProviders = (from p in context.Providers
                                        join u in context.ApplicationUsers on p.UserID equals u.Id
                                        where p.Businesstype == true && u.IsVerified == true
                                        select new
                                        {
                                            p.UserID,
                                            p.BusinessName,
                                            p.Category,
                                            u.PhoneNumber,
                                            u.Address,
                                            AvgPrice = context.Services
                                            .Where(sv => sv.ProviderID == p.UserID)
                                            .Average(pr => (decimal?)pr.Fee) ?? 0
                                        }).ToList();

                ViewBag.Businesses = serviceProviders;
                return View(serviceProviders);
            }
        }


        public IActionResult OrderListProduct(string businessId)
        {
            string userId = userManager.GetUserId(User);
            var user = context.Users.FirstOrDefault(u => u.Id == userId);

            // Check if the user is under review
            if (user != null && user.IsUnderReview == true)
            {
                // Redirect to a page or show a message about the restriction
                TempData["Message"] = "Your account is under review. You cannot access this page at the moment.";
                return View("AccountUnderReview");
            }
            else if (user != null && user.IsVerified == false)
            {
                // Redirect to a page or show a message about the restriction
                TempData["Message"] = "Your account is not yet verified. You cannot access this page at the moment.";
                return View("AccountForVerification");
            }

            else if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Index");
            }

            //var provider = context.Providers.FirstOrDefault(x => x.UserID == businessId);
            var provider = (from p in context.Providers
                            join u in context.ApplicationUsers on p.UserID equals u.Id
                            where p.UserID == businessId
                            select new
                            {
                                p.BusinessName,
                                u.Address
                            }).FirstOrDefault();

            if (provider == null)
            {
                return NotFound();
            }

            var products = context.Products.Where(x => x.ProviderID == businessId).ToList();

            ViewBag.ProviderID = businessId;
            TempData["BusinessName"] = provider.BusinessName;
            ViewBag.Address = provider.Address;

            return View(products);
        }

        public IActionResult AvailListService(string businessId)
        {
            string userId = userManager.GetUserId(User);
            var user = context.Users.FirstOrDefault(u => u.Id == userId);

            // Check if the user is under review
            if (user != null && user.IsUnderReview == true)
            {
                // Redirect to a page or show a message about the restriction
                TempData["Message"] = "Your account is under review. You cannot access this page at the moment.";
                return View("AccountUnderReview");
            }

           else if (user != null && user.IsVerified == false)
            {
                // Redirect to a page or show a message about the restriction
                TempData["Message"] = "Your account is not yet verified. You cannot access this page at the moment.";
                return View("AccountForVerification");
            }

            if (string.IsNullOrEmpty(businessId))
            {
                return RedirectToAction("Index");
            }

            //var provider = context.Providers.FirstOrDefault(x => x.UserID == businessId);
            var provider = (from p in context.Providers
                            join u in context.ApplicationUsers on p.UserID equals u.Id
                            where p.UserID == businessId
                            select new
                            {
                                p.BusinessName,
                                u.Address
                            }).FirstOrDefault();

            if (provider == null)
            {
                return NotFound();
            }

            var services = context.Services.Where(x => x.ProviderID == businessId).ToList();

            ViewBag.ProviderID = businessId;
            TempData["BusinessName"] = provider.BusinessName;
            ViewBag.Address = provider.Address;

            return View(services);
        }

        [HttpGet]
        public IActionResult ConfirmOrder()
        {
            return View(new OrderAvailViewModel());
        }


        private async Task SendNotificationEmail(string email, string providername, OrdersLog order)
        {
            DateTime dateAndTime = order.OrderDate;

            var subject = "Urgent Check your Order Notification";
            //Create a professional HTML body
            //Customize inline styles, text, and branding as needed
            var messageBody = $@"
        <div style=""font-family:Arial,Helvetica,sans-serif;font-size:16px;line-height:1.6;color:#333;"">
            <p>Hi {providername}</p>
            <p> Kindly Check your order notification in <strong>HomeHub</strong>.
             you have pending orders from {order.FirstName} {order.LastName} that need to be processed</p>
            <p>Order Details:</p>
            <p>Item/service: {order.Item}</p>
            <p>Date of Order: {dateAndTime.ToString("dd/MM/yyyy")} </p>    
            <p>Thanks,<br />
            The HomeHub Team</p>
        </div>
    ";
            //Send the Confirmation Email to the User Email Id
            await emailSender.SendEmailAsync(email, subject, messageBody, true);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(OrderAvailViewModel model, string businessId)
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

            var userId = getCurrentUserId; 
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
            entity.UserId = getCurrentUserId;
            entity.FirstName = user.Firstname;
            entity.LastName = user.Lastname;
              entity.lat = user.lat;
            entity.lng = user.lng;
            entity.Address = user.Address;
            entity.ReportId = 1; //remove
            entity.RatingId = 1; //remove
            entity.Quantity = model.qty;
            entity.ModeOfPayment = model.mode;
            entity.Status = "Pending";
            entity.AddInstructions = model.requestatt;

            var maxRatingId = await context.ClientOrders.MaxAsync(o => (int?)o.RatingId) ?? 0;
            entity.RatingId = maxRatingId + 1;

            await context.AddAsync(entity);
            await context.SaveChangesAsync();

            //Create Order Log
            var orderLog = new OrdersLog
            {
                LogId = Guid.NewGuid().ToString(),
                OrderId = entity.ClientId.ToString(),
                OrderDate = entity.OrderDate,
                UserId = getCurrentUserId,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                BusinessId = entity.BusinessId,
                Item = entity.OrderedPs,
                Qty = entity.Quantity,
                Date = entity.Schedule,
                Status = entity.Status,
                Fee = entity.Fee,
                PromoCode = entity.PromoCode,
                PayStatus = "Pending"
            };

            await context.OrdersLogs.AddAsync(orderLog);
            await context.SaveChangesAsync();

            // **Create a notification for the provider**
            var notification = new Notification
            {
                BusinessId = businessId,
                Message = $"A new order has been placed by {user.Firstname} {user.Lastname}.",
                IsRead = false,
            };

            

            var provider = await context.ApplicationUsers.FirstOrDefaultAsync(p => p.Id == businessId);

            var details = await context.Providers.FirstOrDefaultAsync(p => p.UserID == businessId);

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();
            await SendNotificationEmail(provider.Email, details.BusinessName, orderLog);
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
                //Address = user.Address,
                lat = user.lat,
                lng = user.lng

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
                lat = user.lat,
                lng = user.lng
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

            //user.Address = model.Address; // change to lat lng

            user.lat = model.lat;
            user.lng = model.lng;

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
            var user = context.Users.FirstOrDefault(u => u.Id == userId);



            // Get all rated order IDs and convert them to a HashSet for efficient lookup
            var ratedOrderIds = context.Ratings
                .Select(r => r.OrderId.ToString()) // Convert OrderId to string to match OrdersLog
                .ToHashSet();

            // Cancellation counter
            DateTime threeDaysAgo = DateTime.Now.AddDays(-3);
            DateTime lastReviewDate = user.LastReviewDate ?? DateTime.Now.AddDays(-1);

            int recentCancellations = context.OrdersLogs
                .Count(o => o.UserId == userId && o.Status == "Cancelled" && o.Date >= threeDaysAgo && o.Date > lastReviewDate);

            // Cancellation warnings
            if (recentCancellations == 3)
            {
                ViewBag.Warning = "Canceling too many orders can negatively affect your credibility.";
            }
            else if (recentCancellations == 4)
            {
                ViewBag.Warning = "You have canceled 4 orders in the past 3 days. One more and your account will be put under review.";
            }
            else if (recentCancellations == 5)
            {
                if (user.LastReviewDate?.Date == DateTime.Now.Date)
                {
                    ViewBag.Warning = "You've recently had your restriction lifted. Please avoid canceling more orders.";
                }
                else
                {
                    ViewBag.Warning = "You have canceled 5 orders in the past 3 days. Your account is now under review. Some features have been disabled";
                    user.IsUnderReview = true;
                    user.LastReviewDate = DateTime.Now;
                    context.SaveChanges();
                }
            }

            var orders = context.OrdersLogs
                .Where(o => o.UserId == userId)
                .ToList() 
                .Select(o => new OrdersLog
                {
                    LogId = o.LogId,
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    UserId = o.UserId,
                    FirstName = o.FirstName,
                    LastName = o.LastName,
                    BusinessId = o.BusinessId,
                    Item = o.Item,
                    Qty = o.Qty,
                    Date = o.Date,
                    Status = o.Status,
                    Fee = o.Fee,
                    PromoCode = o.PromoCode,
                    PayStatus = o.PayStatus,
                    ProviderName = context.Providers
                        .Where(b => b.UserID == o.BusinessId)
                        .Select(b => b.BusinessName)
                        .FirstOrDefault() ?? "Unknown Provider",
                    IsRated = ratedOrderIds.Contains(o.OrderId)
                })
                .OrderByDescending(o => o.OrderId)
                .ToList();

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

            var eligibleUserIds = await context.Providers
                .Where(b => b.Businesstype == false)
                .Select(b => b.UserID)
                .ToListAsync();

            var refundList = new List<RefundRequest>();

            foreach (var orderLog in deliveredOrders)
            {
                string businessId = orderLog.BusinessId;

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
                PromoCode = orderLog.PromoCode,
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

        public async Task<IActionResult> SellerProfile(string businessId)
        {
            var seller = (from p in context.Providers
                          join u in context.ApplicationUsers on p.UserID equals u.Id
                          where p.UserID == businessId
                          select new SellerProfileVM
                          {
                              UserID = p.UserID,
                              BusinessName = p.BusinessName,
                              Category = p.Category,
                              PhoneNumber = u.PhoneNumber,
                              Address = u.Address,
                              FirstName = u.Firstname,  
                              LastName = u.Lastname,    
                              Email = u.Email           
                          }).FirstOrDefault();

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

            var clientOrder = context.ClientOrders.FirstOrDefault(co => co.ClientId.ToString() == orderLog.OrderId);

            if (clientOrder == null)
            {
                TempData["ErrorMessage"] = "No corresponding client order found.";
                return RedirectToAction("ViewOrders");
            }

            var business = context.Providers.FirstOrDefault(b => b.UserID == clientOrder.BusinessId);
            if (business != null)
            {
                ViewBag.BusinessName = business.BusinessName;
            }

            return View("RateProvider", clientOrder);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitRating(int score, string comments, string clientId, string businessId)
        {
            var userId = await GetCurrentUserId();

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
                Date = DateTime.Now,
                CustomerId = userId,
                ReviewerId = userId
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

                var business = context.Providers.FirstOrDefault(b => b.UserID == clientOrder.BusinessId);
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
            try
            {
                var orderLog = context.OrdersLogs.FirstOrDefault(o => o.LogId == LogId);


                decimal totalAmount = orderLog.Fee;
                string currency = "PHP";

                List<PayMayaVM.PayMayaItem> items = new List<PayMayaVM.PayMayaItem>
                {
                    new PayMayaVM.PayMayaItem
                    {
                        name = orderLog.Item,
                        totalAmount = new PayMayaAmount {value = totalAmount, currency = currency}
                    }
                };

                string response = await _payMayaService.CreateCheckoutAsync(totalAmount, currency, items, LogId);

                //redirect user to Maya site
                var checkoutData = JsonConvert.DeserializeObject<dynamic>(response);
                string checkoutUrl = checkoutData.redirectUrl; // Adjust this based on PayMaya's actual response format

                if (string.IsNullOrEmpty(checkoutUrl))
                {
                    return BadRequest("Failed to retrieve PayMaya payment URL.");
                }

                return Redirect(checkoutUrl);

            }
            catch (Exception ex)
            {
                return Content("Error" + ex.Message);
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
                        order.Fee = 0;
                    }

                    if (clientOrder != null && clientOrder.Status != "Cancelled")
                    {
                        clientOrder.Status = "Cancelled";
                        clientOrder.Fee = 0;
                    }

                    context.SaveChanges();
                }
            }
            return RedirectToAction("ViewOrders");
        }

        public IActionResult ReviewRating(string LogId)
        {
            var orderLog = context.OrdersLogs.FirstOrDefault(o => o.LogId == LogId);

            if (orderLog == null)
            {
                TempData["ErrorMessage"] = "Order not found for the given Log ID.";
                return RedirectToAction("ViewOrders");
            }

            var rating = (from r in context.Ratings
                          join b in context.Providers on r.BusinessId equals b.UserID
                          where r.OrderId == orderLog.OrderId
                          select new
                          {
                              r.OrderId,
                              r.Score,
                              r.Comments,
                              b.BusinessName
                          }).FirstOrDefault();

            if (rating == null)
            {
                TempData["ErrorMessage"] = "No rating found for this order.";
                return RedirectToAction("ViewOrders");
            }

            var model = new ReviewRatingVM
            {
                OrderId = rating.OrderId,
                Score = rating.Score,
                Comments = rating.Comments,
                BusinessName = rating.BusinessName
            };

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

        public async Task<IActionResult> ReviewReport(string id)
        {
            var report = context.Reports.FirstOrDefault(r => r.ReportId == id);

            if (report == null)
            {
                return await Index();
            }

            return View(report);
        }

        public async Task<IActionResult> PaymentSuccess(string requestReferenceNumber)
        {
            if (!string.IsNullOrEmpty(requestReferenceNumber))
            {
                await UpdateOrderStatus(requestReferenceNumber, "Paid");
            }

            ViewBag.Message = "Payment was successful!";
            return View();
        }

        public async Task<IActionResult> PaymentFailure(string requestReferenceNumber)
        {
            if (!string.IsNullOrEmpty(requestReferenceNumber))
            {
                await UpdateOrderStatus(requestReferenceNumber, "Pending");
            }

            ViewBag.Message = "Payment failed!";
            return View();
        }

        public async Task<IActionResult> PaymentCancel(string requestReferenceNumber)
        {
            if (!string.IsNullOrEmpty(requestReferenceNumber))
            {
                await UpdateOrderStatus(requestReferenceNumber, "Pending");
            }

            ViewBag.Message = "Payment was cancelled!";
            return View();
        }

        private async Task UpdateOrderStatus(string logId, string status)
        {
            var orderLog = await context.OrdersLogs.FirstOrDefaultAsync(o => o.LogId == logId);
            if (orderLog != null)
            {
                orderLog.PayStatus = status;
                await context.SaveChangesAsync();
            }
        }
    }
}