using AutoMapper;
using HomeHub.App.Models;
using HomeHub.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace HomeHub.App.Controllers
{
    public class CustomerController : Controller
    {

        private readonly HomeHubContext context;

        public CustomerController(HomeHubContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult OrderProduct()
        {
            var categories = context.Businesses
                .Where(b => b.BusinessType == '0')
                .Select(b => b.OfferList) 
                .Distinct()
                .ToList();

            ViewBag.Categories = categories;

            //To only show Product Providers
            List<Business> list = context.Businesses.Where(x => x.BusinessType == '0').ToList();
            ViewBag.Businesses = list; // Store businesses in ViewBag
            return View(list);
        }

        public IActionResult AvailService()
        {
            // Get the unique categories from the OfferList in the Businesses table
            var categories = context.Businesses
                .Where(b => b.BusinessType == '1')
                .Select(b => b.OfferList) 
                .Distinct()
                .ToList();

            ViewBag.Categories = categories;    

            //To only show Service Providers
            List<Business> list = context.Businesses.Where(x => x.BusinessType == '1').ToList();
            ViewBag.Businesses = list; // Store businesses in ViewBag
            return View(list);
        }

            
            public IActionResult OrderListProduct(int id)
            {
                //To only show only the chosen provider's products
                List<Product> list = context.Products.Where(x => x.ProviderID == id).ToList();
                return View(list);
            } 

            public IActionResult AvailListService(int id)
        {
            //To only show only the chosen provider's services
            List<Service> list = context.Services.Where(x => x.ProviderID == id).ToList();
            return View(list);
        }

        [HttpGet]
        public IActionResult ConfirmOrder()
        {
            return View(new OrderAvailViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(OrderAvailViewModel model, int id)
        {
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

            ClientOrder entity = new ClientOrder();
            //business not yet passed
            entity.BusinessId = id.ToString();
            entity.OrderDate = DateTime.Parse(model.ddeliv);
            entity.Schedule = DateTime.Parse(model.tdeliv);
            entity.OrderedPs = model.chosen;
            //entity.Fee = Convert.ToDecimal(model.price);  uncomment if fee below does not work
            entity.Fee = TotalPrice;
            entity.PromoCode = model.promo;
            entity.UserId = 3; //temporary userID
            //entity.UserId = int.Parse(model.userID);
            //trying to figure out
            //entity.RatingId = 1 + entity.ClientId;
            //entity.ReportId = 1 + entity.ClientId;
            entity.Quantity = model.qty;
            entity.ModeOfPayment = model.mode;
            await context.AddAsync(entity);
            await context.SaveChangesAsync();


            model.discount = Discount;
            model.totalPrice = TotalPrice;

            //Snackbar Message
            TempData["SnackbarMessage"] = "Your order has been placed";

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
            List<OrdersLog> list = context.OrdersLogs.ToList();
            return View(list);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult InitiateRefund()
        {
            return View(new RefundViewModel());
        }

        [HttpPost]
        public ActionResult Refund(RefundViewModel model)
        {
            // Find the order using the unique fields
            var order = context.ClientOrders
                .FirstOrDefault(o => o.ClientId == model.ClientID &&
                                     o.BusinessId == model.BusinessID &&
                                     o.OrderDate == model.OrderDate &&
                                     o.UserId == model.UserID);

            if (order == null)
            {
                // Handle case where the order does not exist
                ModelState.AddModelError("", "Order not found.");
                return View("InitiateRefund", model);
            }

            // Process the refund
            order.Status = false; // Assuming false means refunded
            context.SaveChanges();

            // Redirect or show a confirmation message
            return RedirectToAction("Index"); // Redirect to an appropriate view
        }

    }
}