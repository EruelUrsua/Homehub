using HomeHub.App.Models;
using HomeHub.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeHub.App.Controllers
{
    public class ProviderController : Controller
    {
        private readonly HomeHubContext _context;

        public ProviderController(HomeHubContext context)
        {
            _context = context;
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
    }
}
