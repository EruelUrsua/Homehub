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
                    
            var userId = 1; //Will replace with logged-in user id retrieval logic || input simular userID to a customer userID
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
            entity.UserId = 1; //temporary userID
            entity.FirstName = user.Firstname;
            entity.LastName = user.Lastname;
            //entity.UserId = int.Parse(model.userID);
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

        private (decimal OriginalFee, decimal DiscountAmount) CalculateDiscount(string promoCode, decimal discountedFee)
        {
            var promo = context.Promos.FirstOrDefault(p => p.PromoCode == promoCode && p.PromoEnd > DateTime.Now);
            if (promo != null)
            {
                // Reconstruct the original fee
                decimal originalFee = discountedFee / (1 - promo.Discount);

                // Calculate the discount amount
                decimal discountAmount = originalFee - discountedFee;

                return (originalFee, discountAmount);
            }

            // If no promo, the discounted fee is the original fee, and the discount amount is 0
            return (discountedFee, 0);
        }

        public async Task<IActionResult> ShowEligibleOrdersForRefund(int clientId)
        {
            // Fetch accepted orders from the OrdersLog
            var eligibleRefunds = await context.OrdersLogs
                .Where(log => log.Status == "Accepted")
                .Select(log => new
                {
                    log.LogId,
                    log.OrderDate,
                    log.Item,
                    log.Qty,
                    log.BusinessId,
                    log.OrderId,
                    // Use business id and order date to find associated ClientOrders
                    ClientOrder = context.ClientOrders
                        .FirstOrDefault(o => o.BusinessId == log.BusinessId && o.OrderDate == log.OrderDate)
                })
                .ToListAsync();

            var refundList = new List<RefundVM>();

            foreach (var x in eligibleRefunds.Where(x => x.ClientOrder != null))
            {
                var refund = new RefundVM
                {
                    ClientID = clientId,
                    BusinessID = x.ClientOrder.BusinessId,
                    OrderDate = x.ClientOrder.OrderDate,
                    OrderedPs = x.Item,
                    Quantity = x.Qty,
                    OrderId = x.OrderId,
                    Fee = x.ClientOrder.Fee,
                    PromoCode = x.ClientOrder.PromoCode
                };

                if (!string.IsNullOrEmpty(refund.PromoCode))
                {
                    // Calculate the original fee and discount amount
                    var (originalFee, discountAmount) = CalculateDiscount(refund.PromoCode, refund.Fee);

                    // Assign the calculated values
                    refund.OriginalFee = originalFee;
                    refund.DiscountAmount = discountAmount;
                    refund.DiscountedFee = refund.Fee;

                    var promo = context.Promos.FirstOrDefault(p => p.PromoCode == refund.PromoCode);
                    refund.DiscountPercentage = promo != null ? promo.Discount * 100 : 0;
                }
                else
                {
                    // If no promo, the discounted fee is the original fee
                    refund.OriginalFee = refund.Fee;
                    refund.DiscountedFee = refund.Fee;
                    refund.DiscountPercentage = 0;
                    refund.DiscountAmount = 0;
                }

                refundList.Add(refund);
            }

            return View(refundList);
        }

        [HttpPost]
        public async Task<IActionResult> Refund(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                TempData["Message"] = "Invalid Order ID.";
                return RedirectToAction("ShowEligibleOrdersForRefund");
            }

            // Find the order based on the orderId in the OrdersLog table
            var order = await context.OrdersLogs.FirstOrDefaultAsync(o => o.OrderId == orderId && o.Status == "Accepted");

            if (order != null)
            {
                // Check if the order is eligible for refund
                if (order.Status != "Refunded" && order.Status != "Cancelled")
                {
                    order.Status = "Refund Requested";

                    // Save changes to the database
                    await context.SaveChangesAsync();

                    TempData["Message"] = "Your refund request has been submitted successfully.";
                }
                else
                {
                    TempData["Message"] = "Refund request cannot be processed for this order.";
                }
            }
            else
            {
                TempData["Message"] = "Order not found.";
            }

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

            decimal amount = orderLog.Fee; // Example amount
            string orderRef = "ORDER-123456"; // Unique order ID

            string qrCodeUrl = await _payMayaService.PayOnline(amount, orderRef);

            if (!string.IsNullOrEmpty(qrCodeUrl))
            {
                ViewBag.QRCodeUrl = qrCodeUrl;
                return View(); // Show the QR code in a view
            }

            return View("Error");
        }
    }
}