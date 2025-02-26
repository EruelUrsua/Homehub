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

        public IActionResult ProviderHome()
        {
            return View();
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

            Product entity = new Product
            {
                ProductId = model.ProductId,
                ProductItem = model.ProductItem,
                Qty = model.Qty,
                Price = model.Price,
                ContainerType = model.ContainerType,
                ProviderID = providerId,
                ProductImagePath = "/images/" + uniqueFileName // Save file path
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
                return View(model);  
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

        [HttpGet]
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
                ProviderID = product.ProviderID,
                ExistingImage = product.ProductImagePath // Store the image file path
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
                return View(model);  
            }

            var product = await _context.Products.FindAsync(model.ProductId);
            if (product == null)
                return RedirectToAction("ProductsView");

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
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ProductImage.FileName;
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
                return View(model);  
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
            var productOrders = await (from order in _context.ClientOrders
                                       join business in _context.Businesses
                                       on order.BusinessId equals business.UserID // Manual join
                                       where business.Businesstype == '0'
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
                                       }).ToListAsync();


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
            var serviceRequests = await (from order in _context.ClientOrders
                                       join business in _context.Businesses
                                       on order.BusinessId equals business.UserID // Manual join
                                       where business.Businesstype == '1'
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
                                       }).ToListAsync();


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

            // Log the accepted order into OrdersLog
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

            // Log the accepted order into OrdersLog
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
            };

            _context.OrdersLogs.Add(orderLog);
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
            var logs = await _context.OrdersLogs
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

        public async Task<IActionResult> ShowNotifications(int businessId)
        {
            businessId = 3;

            var notifications = await _context.Notifications
                .Where(n => n.BusinessId == businessId)
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
            int businessId = 1;

            // Get refund requests for this provider's BusinessId
            var refundList = await _context.RefundRequests
                .Where(r => r.BusinessId == businessId)
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

            var orderLog = await _context.OrdersLogs
                .FirstOrDefaultAsync(log => log.OrderId == refundRequest.OrderId);

            if (orderLog != null)
            {
                orderLog.Status = "Refund Accepted"; // Update order history
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
            }

            refundRequest.RefundStatus = "Refund Rejected";
            refundRequest.RefundActionDate = DateTime.Now;
            refundRequest.RejectionReason = rejectionReason;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Refund request has been rejected successfully.";
            return RedirectToAction("ShowRefundRequests");
        }

        public IActionResult ManagePromo()
        {
            List<Promo> list = _context.Promos.ToList();
            return View(list);
        }

        public IActionResult CreatePromo()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreatePromo(PromoViewModel model)
        {
            if (ModelState.IsValid)
            {
                Promo entity = new Promo();
                entity.PromoName = model.PromoName;
                entity.PromoCode = model.PromoCode;
                entity.PromoStart = model.PromoStart;
                entity.PromoEnd = model.PromoEnd;
                entity.BusinessName = model.BusinessName;
                entity.Discount = model.Discount;

                _context.Promos.Add(entity);
                _context.SaveChanges();

                return RedirectToAction("ProviderHome", "Provider");
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
                BusinessName = promo.BusinessName,
                Discount = promo.Discount
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

                promo.PromoName = promoViewModel.PromoName;
                promo.PromoCode = promoViewModel.PromoCode;
                promo.PromoStart = promoViewModel.PromoStart;
                promo.PromoEnd = promoViewModel.PromoEnd;
                promo.BusinessName = promoViewModel.BusinessName;
                promo.Discount = promoViewModel.Discount;

                _context.Promos.Update(promo);
                await _context.SaveChangesAsync();

                return RedirectToAction("ManagePromo");
            }
            return View(promoViewModel);
        }
    }
}
