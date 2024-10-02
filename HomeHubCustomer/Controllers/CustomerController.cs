using AutoMapper;
using HomeHub.App.Models;
using HomeHub.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;

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
            //To only show Product Providers
            List<Business> list = context.Businesses.Where(x => x.BusinessType == '0').ToList();
            return View(list);
        }

        public IActionResult AvailService()
        {
            //To only show Service Providers
            List<Business> list = context.Businesses.Where(x => x.BusinessType == '1').ToList();
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

        /*
        public IActionResult OrderSummary()
        {
            return View();
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
            ClientOrder entity = new ClientOrder();
            //business not yet passed
            entity.BusinessId = id.ToString();
            entity.OrderDate = DateTime.Parse(model.ddeliv);  
            entity.Schedule = DateTime.Parse(model.tdeliv);
            entity.OrderedPs = model.chosen;
            entity.Fee = Convert.ToDecimal(model.price);
            entity.PromoCode = model.promo;
                //temporary userID
            entity.UserId = 3;
            //entity.UserId = int.Parse(model.userID);
                //trying to figure out
            //entity.RatingId = 1 + entity.ClientId;
            //entity.ReportId = 1 + entity.ClientId;
            entity.Quantity = model.qty;
            entity.ModeOfPayment = model.mode;
            await context.AddAsync(entity);
            await context.SaveChangesAsync();


            //Snackbar Message
            TempData["SnackbarMessage"] = "Your order has been placed";

            return RedirectToAction("Index");
        }
        */

        [HttpGet]
        public IActionResult ConfirmOrder()
        {
            return View(new OrderAvailViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(OrderAvailViewModel model, int id)
        {
            ClientOrder entity = new ClientOrder();
            //business not yet passed
            entity.BusinessId = id.ToString();
            entity.OrderDate = DateTime.Parse(model.ddeliv);
            entity.Schedule = DateTime.Parse(model.tdeliv);
            entity.OrderedPs = model.chosen;
            entity.Fee = Convert.ToDecimal(model.price);
            entity.PromoCode = model.promo;
            //temporary userID
            entity.UserId = 3;
            //entity.UserId = int.Parse(model.userID);
            //trying to figure out
            //entity.RatingId = 1 + entity.ClientId;
            //entity.ReportId = 1 + entity.ClientId;
            entity.Quantity = model.qty;
            entity.ModeOfPayment = model.mode;
            await context.AddAsync(entity);
            await context.SaveChangesAsync();


            //Snackbar Message
            TempData["SnackbarMessage"] = "Your order has been placed";

            return View("OrderSummary", model);
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
    }
}