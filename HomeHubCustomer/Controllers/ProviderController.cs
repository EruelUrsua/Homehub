using HomeHub.App.Models;
using HomeHub.DataModel;
using HomeHub.DataModel.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace HomeHub.App.Controllers
{
    public class ProviderController : Controller
    {
        private readonly HomeHubContext _context;
        private readonly IOrderService _orderService;
        private readonly IRepository<ClientOrder> _orderRepository;
        
        public ProviderController(HomeHubContext context, IOrderService orderService, IRepository<ClientOrder> orderRepository)
        {
            _context = context;
            _orderService = orderService;
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> ProductsServices()
        {
            var viewModel = new CombinedViewModel
            {
                Products = await _context.Products.ToListAsync(),
                Services = await _context.Services.ToListAsync()
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            return View(new ProductServiceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductServiceViewModel model)
        {
            Product entity = new Product();
            entity.ProductId = model.ProductId;
            entity.ProductItem = model.ProductItem;
            entity.Qty = model.Qty; 
            entity.Price = model.Price; 
            entity.ContainerType = model.ContainerType; 
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction("ProductsServices");
        }

        [HttpGet]
        public IActionResult AddService()
        {
            return View(new ProductServiceViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService(ProductServiceViewModel model)
        {
            Service entity = new Service();
            entity.ServiceId = model.ServiceId;
            entity.ServiceItem = model.ServiceItem;
            entity.Details = model.Details;
            entity.Fee = model.Fee;
            entity.Available = model.Available;
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction("ProductsServices");
        }

        public async Task<IActionResult> UpdateProduct(string? id)
        {
            var product = await _context.Products.FindAsync(id);
            var viewModel = new ProductServiceViewModel
            {
                ProductId = product.ProductId,
                ProductItem = product.ProductItem,
                Qty = product.Qty,
                Price = product.Price,
                ContainerType = product.ContainerType
            };

            if (product == null) return RedirectToAction("ProductsServices");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProduct(Product model)
        {
            _context.Set<Product>().Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("ProductsServices");
        }

        public async Task<IActionResult> UpdateService(string? id)
        {
            var service = await _context.Services.FindAsync(id);
            var viewModel = new ProductServiceViewModel
            {
                ServiceId = service.ServiceId,
                ServiceItem = service.ServiceItem,
                Details = service.Details,
                Fee = service.Fee,
                Available = service.Available
            };

            if (service == null) return RedirectToAction("ProductsServices");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateService(Service model)
        {
            _context.Set<Service>().Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("ProductsServices");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Set<Product>().Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("ProductsServices");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveService(string id)
        {
            var service = await _context.Services.FindAsync(id);
            _context.Set<Service>().Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction("ProductsServices");
        }

        public async Task<IActionResult> Orders()
        {
            if (_orderRepository == null)
            {
                // Log or handle the fact that _orderRepository is not initialized
                return RedirectToAction("Error");
            }

            var orders = await _orderRepository.GetAllAsync();
            var viewModel = orders.Select(order => new ProviderAcceptOrderViewModel
            {
                ClientId = order.ClientId,
                BusinessId = order.BusinessId ?? string.Empty,
                OrderDate = order.OrderDate,
                Schedule = order.Schedule,
                OrderedPs = order.OrderedPs ?? string.Empty,
                Fee = order.Fee,
                Status = order.Status,
                PromoCode = order.PromoCode ?? string.Empty,
                UserId = order.UserId,
                RatingId = order.RatingId,
                ReportId = order.ReportId,
                Quantity = order.Quantity,
                ModeOfPayment = order.ModeOfPayment ?? string.Empty
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOrder (int clientId)
        {
            var order = await _orderRepository.GetByIdAsync(clientId);

            order.Status = true;
            await _orderService.AcceptOrderAsync(order);

            return RedirectToAction("Orders");
        }
    }
 }
