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

        /*
        [HttpGet]
        public IActionResult OrderListProduct()
        {
            return View(new OrderAvailViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderListProduct(OrderAvailViewModel model, int id)
        {
            ClientOrder entity = new ClientOrder();
            entity.BusinessId = id.ToString();
            entity.OrderDate = DateTime.Parse(model.ddeliv);
            entity.Schedule = DateTime.Parse(model.tdeliv);
            entity.Fee = Int32.Parse(model.price);
            entity.PromoCode = model.promo;
            //temporary userID
            entity.UserId = 1;
            entity.Quantity = model.qty;
            entity.ModeOfPayment = model.mode;
            await context.AddAsync(entity);
            await context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        */
            
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
            ClientOrder entity = new ClientOrder();
            entity.BusinessId = id.ToString();
            entity.OrderDate = DateTime.Parse(model.ddeliv);  
            entity.Schedule = DateTime.Parse(model.tdeliv);
            entity.Fee = Convert.ToDecimal(model.price);
            entity.PromoCode = model.promo;
            //temporary userID
            entity.UserId = 1;
            entity.Quantity = model.qty;
            entity.ModeOfPayment = model.mode;
            await context.AddAsync(entity);
            await context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult UserProfile()
        {
            return View();
        }

        public IActionResult Orders()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}