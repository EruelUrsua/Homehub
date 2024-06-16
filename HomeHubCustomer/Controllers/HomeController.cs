using AutoMapper;
using HomeHub.App.Models;
using HomeHub.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;

namespace HomeHub.App.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        private readonly HomeHubContext context;
        //private readonly IMapper mapper;

        public HomeController(HomeHubContext context)
        {
            this.context = context;
        }



        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}


        /*public async Task<IActionResult> OrderProduct()
        {
            var viewModel = new ProductProviderVM();
            {
                ProviderName = await _context.Businesses.ToListAsync(),
                ProviderType = await _context.Businesses.ToListAsync()
            }

            return View(viewModel);
        }*/


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