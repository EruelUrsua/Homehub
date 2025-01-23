using HomeHub.App.Models;
using HomeHub.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public IActionResult ProductsView(int? businessId)
        {
            ViewBag.Businesses = _context.Businesses
            .Where(b => b.Businesstype == '0') // Filter only product providers
            .ToList();

            ViewBag.ProviderID = businessId;

            if (businessId.HasValue)
            {
                var products = _context.Products
                    .Where(p => p.ProviderID == businessId.Value)
                    .ToList();

                ViewBag.BusinessName = _context.Businesses
                    .FirstOrDefault(b => b.UserID == businessId.Value)?.BusinessName;

                return View(products);
            }

            return View(new List<Product>()); // Empty list if no business is selected
        }

        // Services View
        public IActionResult ServicesView(int? businessId)
        {
            ViewBag.Businesses = _context.Businesses
            .Where(b => b.Businesstype == '1') // Filter only service providers
            .ToList();

            ViewBag.ProviderID = businessId;

            if (businessId.HasValue)
            {
                var services = _context.Services
                    .Where(s => s.ProviderID == businessId.Value)
                    .ToList();

                ViewBag.BusinessName = _context.Businesses
                    .FirstOrDefault(b => b.UserID == businessId.Value)?.BusinessName;

                return View(services);
            }

            return View(new List<Service>()); // Empty list if no business is selected
        }

        [HttpGet]
        public IActionResult AddProduct(int providerId)
        {
            // Ensure you have a valid provider ID
            ViewBag.ProviderID = providerId;

            ViewBag.Businesses = _context.Businesses
               .Where(b => b.Businesstype == '0') // Only Product Providers
               .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductVM model)
        {
            // Ensure the ProviderID is captured from the ViewBag (or use a hidden field)
            var providerId = model.ProviderID;  // This will come from the form or ViewBag

            if (!ModelState.IsValid)
            {
                return View(model);  // If validation fails, return the same view
            }

            Product entity = new Product
            {
                ProductId = model.ProductId,
                ProductItem = model.ProductItem,
                Qty = model.Qty,
                Price = model.Price,
                ContainerType = model.ContainerType,
                ProviderID = providerId 
            };

            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction("ProductsView", new { businessId = model.ProviderID });
        }

        [HttpGet]
        public IActionResult AddService()
        {
            ViewBag.Businesses = _context.Businesses
               .Where(b => b.Businesstype == '1') // Only Service Providers
               .ToList();

            return View(new ServiceVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService(ServiceVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);  // If validation fails, return the same view
            }

            Service entity = new Service
            {
                ServiceId = model.ServiceId,
                ServiceItem = model.ServiceItem,
                Details = model.Details,
                Fee = model.Fee,
                Available = model.Available,
                ProviderID = model.ProviderID
            };

            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction("ServicesView", new { businessId = model.ProviderID });
        }

        public async Task<IActionResult> UpdateProduct(string? id)
        {
            var product = await _context.Products.FindAsync(id);
            var viewModel = new ProductVM
            {
                ProductId = product.ProductId,
                ProductItem = product.ProductItem,
                Qty = product.Qty,
                Price = product.Price,
                ContainerType = product.ContainerType,
                ProviderID= product.ProviderID
            };

            ViewBag.Businesses = _context.Businesses
                .Where(b => b.Businesstype == '0') // Only Product Providers
                .ToList();


            if (id == null || product == null) return RedirectToAction("ProductsView");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProduct(ProductVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);  // If validation fails, return the same view
            }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null)
                return RedirectToAction("ProductsView");

            product.ProductItem = model.ProductItem;
            product.Qty = model.Qty;
            product.Price = model.Price;
            product.ContainerType = model.ContainerType;
            product.ProviderID = model.ProviderID;

            _context.Set<Product>().Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("ProductsView", new { businessId = model.ProviderID });
        }

        public async Task<IActionResult> UpdateService(string? id)
        {
            var service = await _context.Services.FindAsync(id);
            var viewModel = new ServiceVM
            {
                ServiceId = service.ServiceId,
                ServiceItem = service.ServiceItem,
                Details = service.Details,
                Fee = service.Fee,
                Available = service.Available,
                ProviderID = service.ProviderID
            };

            ViewBag.Businesses = _context.Businesses
                .Where(b => b.Businesstype == '1') // Only Service Providers
                .ToList();

            if (id == null || service == null) return RedirectToAction("ServicesView");

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateService(ServiceVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);  // If validation fails, return the same view
            }

            var service = await _context.Services.FindAsync(model.ServiceId);
            if (service == null)
                return RedirectToAction("ServicesView");

            service.ServiceItem = model.ServiceItem;
            service.Details = model.Details;
            service.Fee = model.Fee;
            service.Available = model.Available;
            service.ProviderID = model.ProviderID;

            _context.Set<Service>().Update(service);
            await _context.SaveChangesAsync();

            return RedirectToAction("ServicesView", new { businessId = model.ProviderID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Set<Product>().Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("ProductsView");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveService(string id)
        {
            var service = await _context.Services.FindAsync(id);
            _context.Set<Service>().Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction("ServicesView");
        }
    }
}
