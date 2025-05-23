﻿using HomeHub.App.Models;
using HomeHub.DataModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace HomeHub.App.Controllers
{
    public class ProviderController : Controller
    {
        private readonly HomeHubContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly EmailSenderService emailSender;

        public ProviderController(HomeHubContext context, UserManager<ApplicationUser> userManager, EmailSenderService emailSender)
        {
            _context = context;
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

        public async Task<IActionResult> ProviderHome()
        {
            var userId = await GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var provider = _context.Providers.FirstOrDefault(p => p.UserID == userId);

            if (provider == null)
            {
                return NotFound();
            }

            ViewBag.BusinessType = provider.Businesstype;

            return View("ProviderHome");
        }

        public async Task<IActionResult> ProductsView()
        {
            string userId = await GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // Prevent access if not logged in
            }

            var provider = _context.Providers.FirstOrDefault(p => p.UserID == userId && p.Businesstype == false); // 0 for product providers

            if (provider == null)
            {
                return Forbid(); // Prevent non-providers from accessing this
            }

            ViewBag.BusinessName = provider.BusinessName;
            ViewBag.ProviderID = provider.UserID;

            var products = _context.Products
                .Where(p => p.ProviderID == userId)
                .ToList();

            return View(products);
        }

        public async Task<IActionResult> SalesView()
        {
            var user = await GetCurrentUserId();

            if (user == null) return Unauthorized();

            var provider = await _context.Providers
                .FirstOrDefaultAsync(p => p.UserID == user);

            if (provider == null) return Forbid();
            var sales = _context.OrdersLogs.Where(c => c.BusinessId == user).ToList();

            return View(sales);


            
        }

        public async Task<IActionResult> ServicesView()
        {
            string userId = await GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(); // Prevent access if not logged in
            }

            var provider = _context.Providers.FirstOrDefault(p => p.UserID == userId && p.Businesstype == true); // 1 for service providers

            if (provider == null)
            {
                return Forbid(); // Prevent non-providers from accessing this
            }

            ViewBag.BusinessName = provider.BusinessName;
            ViewBag.ProviderID = provider.UserID;

            var services = _context.Services
                .Where(s => s.ProviderID == userId)
                .ToList();

            return View(services);
        }

        private string GenerateProductId()
        {
            // Retrieve the last Product ID that starts with "P"
            var lastProduct = _context.Products
                .Where(p => p.ProductId.StartsWith("P"))
                .OrderByDescending(p => p.ProductId)
                .FirstOrDefault();

            // Default sequence number if no products exist yet
            int sequenceNumber = 1;

            // Increment the sequence number based on the last Product ID
            if (lastProduct != null)
            {
                string lastId = lastProduct.ProductId.Substring(1); // Remove the "P" prefix
                if (int.TryParse(lastId, out int lastNumber))
                {
                    sequenceNumber = lastNumber + 1;
                }
            }

            return $"P{sequenceNumber:D3}"; // Format as P001, P002, P003...
        }


        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            // Get the logged-in provider's UserID
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(); // Ensure the user is logged in
            }

            var provider = _context.Providers.FirstOrDefault(p => p.UserID == user.Id);
            if (provider == null)
            {
                return Forbid(); // User is not a provider
            }

            // Generate new Product ID
            string newProductId = GenerateProductId();

            // Pass generated Product ID and Provider ID to the view
            ViewBag.GeneratedProductId = newProductId;
            ViewBag.ProviderID = provider.UserID;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(ProductVM model)
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var provider = _context.Providers.FirstOrDefault(p => p.UserID == user.Id);

            if (provider == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);  
            }

            if (model.ProductImage == null || model.ProductImage.Length == 0)
            {
                ModelState.AddModelError("ProductImage", "Please upload a product image.");
                return View(model);
            }

            string uniqueFileName = null;

            if (model.ProductImage != null)
            {
                // 1️ Define the folder path
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                // Ensure folder exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 2️ Create a unique file name to avoid overwriting
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProductImage.FileName;

                // 3️ Combine folder path + file name
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 4️ Save the image to the `wwwroot/images/` folder
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProductImage.CopyToAsync(fileStream);
                }
            }

            // Generate unique Product ID
            string newProductId = GenerateProductId();

            Product entity = new Product
            {
                ProductId = newProductId,
                ProductItem = model.ProductItem,
                Qty = model.Qty,
                Price = model.Price,
                ContainerType = model.ContainerType,
                ProviderID = provider.UserID,
                ProductImagePath = "/images/" + uniqueFileName // Save file path
            };

            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction("ProductsView");
        }

        private string GenerateServiceId()
        {
            var lastService = _context.Services
                .OrderByDescending(s => s.ServiceId)
                .FirstOrDefault();

            int nextIdNumber = 1; // Default to S001 if no services exist

            if (lastService != null)
            {
                string lastServiceId = lastService.ServiceId; // Example: "S003"

                // Extract the numeric part (003) and increment
                if (lastServiceId.Length > 1 && int.TryParse(lastServiceId.Substring(1), out int lastNumber))
                {
                    nextIdNumber = lastNumber + 1;
                }
            }

            // Format as "S001", "S002", etc.
            return $"S{nextIdNumber:D3}";
        }

        [HttpGet]
        public async Task<IActionResult> AddService()
        {
            // Get the logged-in provider's UserID
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(); // Ensure the user is logged in
            }

            var provider = _context.Providers.FirstOrDefault(p => p.UserID == user.Id && p.Businesstype == true);

            if (provider == null)
            {
                return Forbid(); // User is not a service provider
            }

            // Generate new Service ID
            string newServiceId = GenerateServiceId();

            // Pass generated Service ID and Provider ID to the view
            ViewBag.GeneratedServiceId = newServiceId;
            ViewBag.ProviderID = provider.UserID;
            return View(new ServiceVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddService(ServiceVM model)
        {
            // Get the logged-in provider's UserID
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized();
            }

            var provider = _context.Providers.FirstOrDefault(p => p.UserID == user.Id && p.Businesstype == true);
            
            if (provider == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(model);  
            }

            // Generate unique Product ID
            string newServiceId = GenerateServiceId();

            Service entity = new Service
            {
                ServiceId = newServiceId,
                ServiceItem = model.ServiceItem,
                Details = model.Details,
                Fee = model.Fee,
                Available = model.Available,
                ProviderID = provider.UserID
            };

            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            return RedirectToAction("ServicesView");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProduct(string? id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("ProductsView");

            var product = await _context.Products.FindAsync(id);

            if (product == null) return RedirectToAction("ProductsView");

            var viewModel = new ProductVM
            {
                ProductId = product.ProductId,
                ProductItem = product.ProductItem,
                Qty = product.Qty,
                Price = product.Price,
                ContainerType = product.ContainerType,
                ProviderID = product.ProviderID,
                ExistingImage = product.ProductImagePath // Store the image file path
            };

            ViewBag.Providers = _context.Providers
                .Where(b => b.Businesstype == false) // Only Product Providers
                .ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProduct(ProductVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);  
            }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null) return RedirectToAction("ProductsView");

            product.ProductItem = model.ProductItem;
            product.Qty = model.Qty;
            product.Price = model.Price;
            product.ContainerType = model.ContainerType;
            product.ProviderID = model.ProviderID;

            /// Handle image update only if a new image is uploaded
            if (model.ProductImage != null && model.ProductImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                // Delete the old image if it exists
                if (!string.IsNullOrEmpty(product.ProductImagePath))
                {
                    string oldImagePath = Path.Combine(uploadsFolder, Path.GetFileName(product.ProductImagePath));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save the new image
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProductImage.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProductImage.CopyToAsync(fileStream);
                }

                // Update the database with the new image path
                product.ProductImagePath = "/images/" + uniqueFileName;
            }
            else
            {
                // Keep the existing image path
                product.ProductImagePath = model.ExistingImage;
            }

            _context.Products.Update(product);
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

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateService(ServiceVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var service = await _context.Services.FindAsync(model.ServiceId);

            if (service == null)
            {
                return NotFound();
            }

            service.ServiceItem = model.ServiceItem;
            service.Details = model.Details;
            service.Fee = model.Fee;
            service.Available = model.Available;
            service.ProviderID = model.ProviderID; 


            _context.Services.Update(service);
            await _context.SaveChangesAsync();

            return RedirectToAction("ServicesView");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProduct(string id)
        {
            var product = await _context.Products.FindAsync(id);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("ProductsView");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveService(string id)
        {
            var service = await _context.Services.FindAsync(id);

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction("ServicesView");
        }

        private (decimal OriginalFee, decimal DiscountAmount) CalculateDiscount(string promoCode, decimal discountedFee)
        {
            var promo = _context.Promos.FirstOrDefault(p => p.PromoCode == promoCode && p.PromoEnd > DateTime.Now);
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

        public async Task<IActionResult> ProductOrders()
        {
            var user = await GetCurrentUserAsync();

            if (user == null) return Unauthorized();

            var prov = await _context.Providers
                .FirstOrDefaultAsync(p => p.UserID == user.Id && p.Businesstype == false);

            if (prov == null)
            {
                return Forbid();
            }

            var productOrders = await (from order in _context.ClientOrders
                                       join provider in _context.Providers
                                       on order.BusinessId equals provider.UserID // Manual join
                                       where provider.Businesstype == false
                                       select new ProviderOrderVM
                                       {
                                           ClientId = order.ClientId,
                                           BusinessId = order.BusinessId,
                                           OrderDate = order.OrderDate,
                                           Schedule = order.Schedule,
                                           OrderedPs = order.OrderedPs,
                                           Fee = order.Fee,
                                           Status = order.Status,
                                           PromoCode = order.PromoCode ?? string.Empty,
                                           UserId = order.UserId,
                                           RatingId = order.RatingId,
                                           ReportId = order.ReportId,
                                           Quantity = order.Quantity,
                                           ModeOfPayment = order.ModeOfPayment,
                                           Address = order.Address, 
                                           lat    = order.lat,
                                           lng = order.lng
                                       }).ToListAsync();

            if (!productOrders.Any()) // Check if the list is empty
            {
                ViewBag.NoOrdersMessage = "No product orders available.";
            }

            foreach (var order in productOrders)
            {
                if (!string.IsNullOrEmpty(order.PromoCode))
                {
                    // Calculate the original fee and discount amount
                    var (originalFee, discountAmount) = CalculateDiscount(order.PromoCode, order.Fee);

                    // Assign the calculated values
                    order.OriginalFee = originalFee;
                    order.DiscountAmount = discountAmount;
                    order.DiscountedFee = order.Fee; // This is already the discounted fee from the database

                    var promo = _context.Promos.FirstOrDefault(p => p.PromoCode == order.PromoCode);
                    order.DiscountPercentage = promo != null ? promo.Discount * 100 : 0; // Assign discount percentage if promo exists
                }
                else
                {
                    // If no promo, the discounted fee is the original fee
                    order.OriginalFee = order.Fee;
                    order.DiscountedFee = order.Fee;
                    order.DiscountPercentage = 0;
                    order.DiscountAmount = 0;
                }
            }

            return View(productOrders);
        }

        public async Task<IActionResult> ServiceRequests()
        {
            var user = await GetCurrentUserAsync();

            if (user == null) return Unauthorized();

            var prov = await _context.Providers
                .FirstOrDefaultAsync(p => p.UserID == user.Id && p.Businesstype == true);

            if (prov == null)
            {
                return Forbid();
            }

            var serviceRequests = await (from order in _context.ClientOrders
                                       join provider in _context.Providers
                                       on order.BusinessId equals provider.UserID // Manual join
                                       where provider.Businesstype == true
                                       select new ProviderOrderVM
                                       {
                                           ClientId = order.ClientId,
                                           BusinessId = order.BusinessId,
                                           OrderDate = order.OrderDate,
                                           Schedule = order.Schedule,
                                           OrderedPs = order.OrderedPs,
                                           Fee = order.Fee,
                                           Status = order.Status,
                                           PromoCode = order.PromoCode ?? string.Empty,
                                           UserId = order.UserId,
                                           RatingId = order.RatingId,
                                           ReportId = order.ReportId,
                                           Quantity = order.Quantity,
                                           ModeOfPayment = order.ModeOfPayment,
                                           Address = order.Address,
                                           lat = order.lat,
                                           lng = order.lng
                                       }).ToListAsync();

            if (!serviceRequests.Any()) // Check if the list is empty
            {
                ViewBag.NoRequestsMessage = "No service requests available.";
            }

            foreach (var order in serviceRequests)
            {
                if (!string.IsNullOrEmpty(order.PromoCode))
                {
                    // Calculate the original fee and discount amount
                    var (originalFee, discountAmount) = CalculateDiscount(order.PromoCode, order.Fee);

                    // Assign the calculated values
                    order.OriginalFee = originalFee;
                    order.DiscountAmount = discountAmount;
                    order.DiscountedFee = order.Fee; // This is already the discounted fee from the database

                    var promo = _context.Promos.FirstOrDefault(p => p.PromoCode == order.PromoCode);
                    order.DiscountPercentage = promo != null ? promo.Discount * 100 : 0; // Assign discount percentage if promo exists
                }
                else
                {
                    // If no promo, the discounted fee is the original fee
                    order.OriginalFee = order.Fee;
                    order.DiscountedFee = order.Fee;
                    order.DiscountPercentage = 0;
                    order.DiscountAmount = 0;
                }
            }

            return View(serviceRequests);
        }

        private Promo GetPromo(string code)
        {
            return _context.Promos.FirstOrDefault(p => p.PromoCode == code && p.PromoEnd > DateTime.Now);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder(int clientId)
        {
            var order = await _context.ClientOrders
                .FirstOrDefaultAsync(o => o.ClientId == clientId && o.Status == "Pending");

            if (order == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductItem == order.OrderedPs); 

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // **Low Inventory Notification**
            int lowStockThreshold = 10;
            if (product.Qty > 0 && product.Qty <= lowStockThreshold)
            {
                TempData["WarningMessage"] = $"Low inventory alert: {product.ProductItem} is running low. Remaining stock: {product.Qty}.";
            }

            // Decrease the inventory
            product.Qty -= order.Quantity;

            // Default values
            decimal discount = 0;
            decimal totalFee = order.Fee;

            // Apply promo if available
            if (!string.IsNullOrWhiteSpace(order.PromoCode))
            {
                var promo = GetPromo(order.PromoCode);
                if (promo != null)
                {
                    discount = promo.Discount; // Get the discount percentage
                }
            }

            // Update the order status and discounted fee
            order.Status = "Processing";
            order.Fee = totalFee;

            /* Log the accepted order into OrdersLog
            var orderLog = new OrdersLog
            {
                LogId = Guid.NewGuid().ToString(),
                OrderId = order.ClientId.ToString(),
                UserId = order.UserId, 
                OrderDate = order.OrderDate,
                FirstName = order.FirstName,
                LastName = order.LastName,
                BusinessId = order.BusinessId,
                Item = order.OrderedPs,
                Qty = order.Quantity,
                Date = DateTime.Now,
                Status = "Processing",
                Fee = totalFee,
                PromoCode = order.PromoCode
            };

            _context.OrdersLogs.Add(orderLog);
            */
            //Update OrderLogs Status
            var orderLog = await _context.OrdersLogs
                .FirstOrDefaultAsync(log => log.OrderId == order.ClientId.ToString());

            if (orderLog != null)
            {
                //Update existing order log
                orderLog.Status = "Processing";
            }
            else
            {
                View("ProductOrders");
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order is now being processed and inventory has been updated successfully.";
            return RedirectToAction(nameof(ProductOrders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsShipped(int clientId)
        {
            var order = await _context.ClientOrders
                .FirstOrDefaultAsync(o => o.ClientId == clientId && o.Status == "Processing");

            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Shipped";

            // Find existing log entry and update the status
            var orderLog = await _context.OrdersLogs
                .FirstOrDefaultAsync(log => log.OrderId == order.ClientId.ToString());

            if (orderLog != null)
            {
                orderLog.Status = "Shipped";
                orderLog.Date = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Order has been shipped.";
            return RedirectToAction(nameof(ProductOrders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsOutForDelivery(int clientId)
        {
            var order = await _context.ClientOrders
                .FirstOrDefaultAsync(o => o.ClientId == clientId && o.Status == "Shipped");

            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Out for Delivery";

            var orderLog = await _context.OrdersLogs
                .FirstOrDefaultAsync(log => log.OrderId == order.ClientId.ToString());

            if (orderLog != null)
            {
                orderLog.Status = "Out for Delivery";
                orderLog.Date = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Order is out for delivery.";
            return RedirectToAction(nameof(ProductOrders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsDelivered(int clientId)
        {
            var order = await _context.ClientOrders
                .FirstOrDefaultAsync(o => o.ClientId == clientId && o.Status == "Out for Delivery");

            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Delivered";

            var orderLog = await _context.OrdersLogs
                .FirstOrDefaultAsync(log => log.OrderId == order.ClientId.ToString());

            if (orderLog != null)
            {
                orderLog.Status = "Delivered";
                orderLog.Date = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Order has been delivered successfully.";
            return RedirectToAction(nameof(ProductOrders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmServiceReq(int clientId)
        {
            var order = await _context.ClientOrders
                .FirstOrDefaultAsync(o => o.ClientId == clientId && o.Status == "Pending");

            if (order == null)
            {
                return NotFound();
            }

            // Default values
            decimal discount = 0;
            decimal totalFee = order.Fee;

            // Apply promo if available
            if (!string.IsNullOrWhiteSpace(order.PromoCode))
            {
                var promo = GetPromo(order.PromoCode);
                if (promo != null)
                {
                    discount = promo.Discount; // Get the discount percentage
                }
            }

            // Update the order status and discounted fee
            order.Status = "Scheduled";
            order.Fee = totalFee;

            /* Log the accepted order into OrdersLog
            var orderLog = new OrdersLog
            {
                LogId = Guid.NewGuid().ToString(),
                OrderId = order.ClientId.ToString(),
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                FirstName = order.FirstName,
                LastName = order.LastName,
                BusinessId = order.BusinessId,
                Item = order.OrderedPs,
                Qty = order.Quantity,
                Date = DateTime.Now,
                Status = "Scheduled",
                Fee = totalFee,
                PromoCode = order.PromoCode
            };*/

            //Update OrderLogs Status
            var orderLog = await _context.OrdersLogs
                .FirstOrDefaultAsync(log => log.OrderId == order.ClientId.ToString());

            if (orderLog != null)
            {
                //Update existing order log
                orderLog.Status = "Processing";
            }
            else
            {
                View("ProductOrders");
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service has been scheduled. Please prepare for the appointment date.";
            return RedirectToAction(nameof(ServiceRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartService(int clientId)
        {
            var order = await _context.ClientOrders
                .FirstOrDefaultAsync(o => o.ClientId == clientId && o.Status == "Scheduled");

            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Ongoing";

            // Find and update the existing OrdersLog entry
            var orderLog = await _context.OrdersLogs
                .FirstOrDefaultAsync(log => log.OrderId == order.ClientId.ToString());

            if (orderLog != null)
            {
                orderLog.Status = "Ongoing";
                orderLog.Date = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service is now Ongoing.";
            return RedirectToAction(nameof(ServiceRequests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteService(int clientId)
        {
            var order = await _context.ClientOrders
                .FirstOrDefaultAsync(o => o.ClientId == clientId && o.Status == "Ongoing");

            if (order == null)
            {
                return NotFound();
            }

            order.Status = "Completed";

            // Find and update the existing OrdersLog entry
            var orderLog = await _context.OrdersLogs
                .FirstOrDefaultAsync(log => log.OrderId == order.ClientId.ToString());

            if (orderLog != null)
            {
                orderLog.Status = "Completed";
                orderLog.Date = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Service has been marked as Completed.";
            return RedirectToAction(nameof(ServiceRequests));
        }

        public async Task<IActionResult> OrdersLog()
        {
            var user = await GetCurrentUserAsync();

            if (user == null) return Unauthorized();

            var provider = await _context.Providers
                .FirstOrDefaultAsync(p => p.UserID == user.Id);

            if (provider == null) return Forbid();

            var logs = await _context.OrdersLogs
                .Where(log => log.BusinessId == provider.UserID) // Filter logs only for the logged-in provider
                .Select(log => new OrdersLogVM
                {
                    LogId = log.LogId,
                    OrderId = log.OrderId,
                    UserId = log.UserId, 
                    OrderDate = log.OrderDate,
                    FirstName = log.FirstName,
                    LastName = log.LastName,
                    BusinessId = log.BusinessId,
                    Item = log.Item,
                    Qty = log.Qty,
                    Date = log.Date,
                    Status = log.Status
                }).ToListAsync();

            return View(logs);
        }


        private async Task SendNotificationEmail(string email, string providername, OrdersLog order,ApplicationUser user)
        {
     
            var subject = "Urgent Check your Order Notification";
            // Create a professional HTML body
            // Customize inline styles, text, and branding as needed
            var messageBody = $@"
        <div style=""font-family:Arial,Helvetica,sans-serif;font-size:16px;line-height:1.6;color:#333;"">
           style=""font-family:Arial,Helvetica,sans-serif;font-size:16px;line-height:1.6;color:#333;"">
            <p>Hi {providername}</p>
            <p> Kindly Check your order notification in <strong>HomeHub</strong>.
             you have pending orders from {order.FirstName}{order.LastName} that need to be processed</p>
            <p>Order Details:</p><br />
            <p>Item/service{order.Item}</p><br />
            <p>Date of Order{order.OrderDate} </p>    
            <p>Thanks,<br />
            The HomeHub Team</p>
        </div>
    ";
            //Send the Confirmation Email to the User Email Id
            await emailSender.SendEmailAsync(email, subject, messageBody, true);
        }

        public async Task<IActionResult> ShowNotifications(string email)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var provider = await _context.Providers
                .FirstOrDefaultAsync(p => p.UserID == user.Id);
            if (provider == null) return Forbid();

            var notifications = await _context.Notifications
                .Where(n => n.BusinessId == provider.UserID)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            return View(notifications); 
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ShowNotifications", new { businessId = notification?.BusinessId });
        }

        public async Task<IActionResult> ShowRefundRequests()
        {
            var user = await GetCurrentUserAsync();

            if (user == null) return Unauthorized();

            var provider = await _context.Providers
                .FirstOrDefaultAsync(p => p.UserID == user.Id);

            if (provider == null) return Forbid();

            // Get refund requests for this provider's BusinessId
            var refundList = await _context.RefundRequests
                 .Where(r => r.BusinessId == provider.UserID) 
                .Select(r => new RefundRequest
                {
                    RefundId = r.RefundId,
                    OrderId = r.OrderId,
                    Item = r.Item,
                    RefundQuantity = r.RefundQuantity,
                    Fee = r.Fee,
                    PromoCode = r.PromoCode,
                    RefundAmount = r.Fee,
                    RefundStatus = r.RefundStatus,
                    RefundReason = r.RefundReason,
                    RejectionReason = r.RejectionReason, 
                    RefundRequestDate = r.RefundRequestDate
                })
                .ToListAsync();

            // Check if refundList is empty and add a message
            if (!refundList.Any())
                {
                    ViewBag.NoRefundRequests = "No refund requests found for your business.";
                }

            return View(refundList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRefund(int refundId)
        {
            var refundRequest = await _context.RefundRequests
                .FirstOrDefaultAsync(r => r.RefundId == refundId);

           

            if (refundRequest == null)
            {
                TempData["ErrorMessage"] = "Refund request not found.";
                return RedirectToAction("ShowRefundRequests");
            }
            ClientOrder co = new ClientOrder();
            var orderLog = await _context.OrdersLogs
                .FirstOrDefaultAsync(log => log.OrderId == refundRequest.OrderId);
        

            if (orderLog != null)
            {
                orderLog.Status = "Refund Accepted"; // Update order history
                orderLog.Fee = 0;
                co.Status = "Refund Accepted";
                co.Fee = 0;
            }

            // Fetch the product(s) in the order
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductItem == orderLog.Item);

            // Return the product back to inventory
            product.Qty += refundRequest.RefundQuantity;

            refundRequest.RefundStatus = "Refund Accepted";
            refundRequest.RefundActionDate = DateTime.Now;
            refundRequest.RefundAmount = refundRequest.Fee;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Refund request has been accepted successfully. {refundRequest.RefundQuantity} item(s) have been returned to inventory.";
            return RedirectToAction("ShowRefundRequests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRefund(int refundId, string rejectionReason)
        {
            var refundRequest = await _context.RefundRequests
                .FirstOrDefaultAsync(r => r.RefundId == refundId);
            ClientOrder co = new ClientOrder();
            if (refundRequest == null)
            {
                TempData["ErrorMessage"] = "Refund request not found.";
                return RedirectToAction("ShowRefundRequests");
            }

            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                TempData["ErrorMessage"] = "Rejection reason is required.";
                return RedirectToAction("ShowRefundRequests");
            }

            var orderLog = await _context.OrdersLogs
                .FirstOrDefaultAsync(log => log.OrderId == refundRequest.OrderId);

            if (orderLog != null)
            {
                orderLog.Status = "Refund Rejected"; // Update order history
                co.Status = "Refund Rejected";
            }

            refundRequest.RefundStatus = "Refund Rejected";
            refundRequest.RefundActionDate = DateTime.Now;
            refundRequest.RejectionReason = rejectionReason;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Refund request has been rejected successfully.";
            return RedirectToAction("ShowRefundRequests");
        }

        public async Task<IActionResult> ManagePromo()
        {
            var user = await GetCurrentUserAsync();

            if (user == null) return Unauthorized();

            List<Promo> list = await _context.Promos
                 .Where(p => p.BusinessId == user.Id)
                 .ToListAsync();

            return View(list);
        }

        public async Task<IActionResult> CreatePromo()
        {
            var user = await GetCurrentUserAsync();

            if (user == null) return Unauthorized();

            var model = new PromoViewModel
            {
                PromoStart = DateTime.Today,
                PromoEnd = DateTime.Today.AddDays(1)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePromo(PromoViewModel model)
        {
            if (model.PromoStart < DateTime.Today)
            {
                ModelState.AddModelError("PromoStart", "Promo start date cannot be in the past.");
            }

            if (model.PromoEnd <= model.PromoStart)
            {
                ModelState.AddModelError("PromoEnd", "Promo end date must be at least one day after the start date.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();

            if (user == null) return Unauthorized();

            var business = _context.Providers.FirstOrDefault(b => b.UserID == user.Id);

            if (business == null)
            {
                ModelState.AddModelError("", "Business not found for the current user.");
                return View(model);
            }

            bool promoExists = await _context.Promos.AnyAsync(p => p.PromoCode == model.PromoCode);
            if (promoExists)
            {
                ModelState.AddModelError("PromoCode", "This promo code is already in use. Please choose a different one.");
                return View(model);
            }


            if (ModelState.IsValid)
            {
                Promo entity = new Promo
                {
                    PromoName = model.PromoName,
                    PromoCode = model.PromoCode,
                    PromoStart = model.PromoStart,
                    PromoEnd = model.PromoEnd,
                    BusinessName = business.BusinessName,
                    Discount = model.Discount /100,
                    BusinessId = user.Id
                };

                _context.Promos.Add(entity);
                await _context.SaveChangesAsync();

                return RedirectToAction("ManagePromo", "Provider");
            }

            return View(model);
        }

        public async Task<IActionResult> EditPromo(int id)
        {
            var promo = await _context.Promos.FindAsync(id);
            if (promo == null)
            {
                return RedirectToAction("ManagePromo");
            }

            var promoViewModel = new PromoViewModel
            {
                PromoID = promo.PromoId,
                PromoName = promo.PromoName,
                PromoCode = promo.PromoCode,
                PromoStart = promo.PromoStart,
                PromoEnd = promo.PromoEnd,
                Discount = promo.Discount * 100
            };

            return View(promoViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditPromo(PromoViewModel promoViewModel)
        {
            if (ModelState.IsValid)
            {
                var promo = await _context.Promos.FindAsync(promoViewModel.PromoID);
                if (promo == null)
                {
                    return RedirectToAction("ManagePromo");
                }

                bool promoExists = await _context.Promos.AnyAsync(p => p.PromoCode == promoViewModel.PromoCode);
                if (promoExists)
                {
                    ModelState.AddModelError("PromoCode", "Promo Code already exists.");
                    return View(promoViewModel);
                }

                //Date validation
                if (promoViewModel.PromoStart < DateTime.Today)
                {
                    ModelState.AddModelError("PromoStart", "Promo start date cannot be earlier than today.");
                    return View(promoViewModel);
                }

                if (promoViewModel.PromoEnd < promoViewModel.PromoStart.AddDays(1))
                {
                    ModelState.AddModelError("PromoEnd", "Promo end date must be at least one day after the start date.");
                    return View(promoViewModel);
                }

                promo.PromoName = promoViewModel.PromoName;
                promo.PromoCode = promoViewModel.PromoCode;
                promo.PromoStart = promoViewModel.PromoStart;
                promo.PromoEnd = promoViewModel.PromoEnd;
                promo.Discount = promoViewModel.Discount / 100;

                _context.Promos.Update(promo);
                await _context.SaveChangesAsync();

                return RedirectToAction("ManagePromo");
            }
            return View(promoViewModel);
        }

        public async Task<IActionResult> InStorePurchase()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Unauthorized();

            var provider = await _context.Providers.FirstOrDefaultAsync(p => p.UserID == user.Id);
            if (provider == null) return Forbid();

            var products = await _context.Products.Where(p => p.ProviderID == provider.UserID).ToListAsync();

            var viewModel = new InStorePurchaseVM
            {
                Products = products.Select(p => new ProductSelectionVM
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductItem,
                    Stock = p.Qty,
                    Price = p.Price
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessInStorePurchase(InStorePurchaseVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("InStorePurchase", model);
            }

            List<string> messages = new List<string>();

            bool isProductSelected = model.Products.Any(p => p.Quantity > 0);

            if (!isProductSelected)
            {
                TempData["ErrorMessage"] = "❌ Please select at least one product before confirming purchase.";
                return View("InStorePurchase", model);
            }

            foreach (var item in model.Products.Where(p => p.Quantity > 0)) // ✅ Process only selected products
            {
                var product = await _context.Products.FindAsync(item.ProductId);

                if (product != null)
                {
                    if (product.Qty >= item.Quantity)
                    {
                        product.Qty -= item.Quantity;
                        _context.Products.Update(product);

                        messages.Add($"✅ {item.Quantity} {product.ProductItem}(s) sold. Remaining stock: {product.Qty}.");

                        if (product.Qty < 10)
                        {
                            messages.Add($"⚠ Warning: {product.ProductItem} is running low on stock! Only {product.Qty} left.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", $"❌ Not enough stock for {product.ProductItem}.");
                        return View("InStorePurchase", model);
                    }
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = string.Join("<br>", messages);

            return RedirectToAction("InStorePurchase");
        }

        [HttpGet]
        public async Task<IActionResult> CustomerReview(string LogId)
        {
            var user = await GetCurrentUserAsync(); 

            if (user == null)
                return Unauthorized(); 

            var orderLog = _context.OrdersLogs.FirstOrDefault(o => o.LogId == LogId);

            if (orderLog == null)
            {
                TempData["ErrorMessage"] = "Order not found for the given Log ID.";
                return RedirectToAction("OrdersLog");
            }

            // Check the provider's business type
            var provider = _context.Providers.FirstOrDefault(p => p.UserID == user.Id);
            if (provider == null)
            {
                TempData["ErrorMessage"] = "You are not authorized to leave a review.";
                return RedirectToAction("OrdersLog");
            }

            bool isServiceProvider = provider.Businesstype; // true = service, false = product

            // Validate order status based on provider type
            if ((isServiceProvider && orderLog.Status != "Completed") ||
                (!isServiceProvider && orderLog.Status != "Delivered"))
            {
                TempData["ErrorMessage"] = "You can only review customers for completed or delivered orders.";
                return RedirectToAction("OrdersLog");
            }

            var existingReview = _context.Ratings
                .FirstOrDefault(r => r.OrderId == orderLog.OrderId && r.ReviewerId == user.Id);

            if (existingReview != null)
            {
                TempData["ErrorMessage"] = "You have already reviewed this customer.";
                return RedirectToAction("OrdersLog");
            }

            var model = new ProviderReviewVM
            {
                OrderId = orderLog.OrderId,
                CustomerId = orderLog.UserId,
                CustomerName = $"{orderLog.FirstName} {orderLog.LastName}"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerReview(ProviderReviewVM model)
        {
            var user = await GetCurrentUserAsync(); 

            if (user == null)
                return Unauthorized(); 

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var newReview = new Rating
            {
                OrderId = model.OrderId,
                BusinessId = user.Id, 
                CustomerId = model.CustomerId,
                ReviewerId = user.Id, // Mark provider as the reviewer
                Score = model.Score,
                Comments = model.Comments,
                Date = DateTime.UtcNow
            };

            _context.Ratings.Add(newReview);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Review successfully submitted!";
            return RedirectToAction("OrdersLog");
        }

        [HttpGet]
        public async Task<IActionResult> ReviewedCustomers()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
                return Unauthorized();

            // Get all reviews submitted by this provider
            var reviews = _context.Ratings
                .Where(r => r.ReviewerId == user.Id)
                .Select(r => new ProviderReviewVM
                {
                    OrderId = r.OrderId,
                    CustomerId = r.CustomerId,
                    CustomerName = _context.Users
                        .Where(u => u.Id == r.CustomerId)
                        .Select(u => u.Firstname + " " + u.Lastname)
                        .FirstOrDefault(),
                    Score = r.Score,
                    Comments = r.Comments
                })
                .ToList();

            return View(reviews);
        }

        public async Task<IActionResult> ViewRatings()
        {
            var providerId = await GetCurrentUserId(); // your BusinessId

            var ratings = await _context.Ratings
                .Where(r => r.BusinessId == providerId)
                .OrderByDescending(r => r.Date)
                .ToListAsync();

            var viewModel = new ProviderRatingsViewModel
            {
                AverageRating = ratings.Any() ? ratings.Average(r => r.Score) : 0,
                Ratings = ratings.Select(r => new RatingItemViewModel
                {
                    Score = r.Score,
                    Comments = r.Comments,
                    Date = r.Date
                }).ToList()
            };

            return View(viewModel);
        }
    }
}
