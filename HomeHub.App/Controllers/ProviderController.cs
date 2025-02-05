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

        public async Task<IActionResult> Orders()
        {
            var orders = await _context.ClientOrders
                .Select(o => new ProviderOrderVM
                {
                    ClientId = o.ClientId,
                    BusinessId = o.BusinessId,
                    OrderDate = o.OrderDate,
                    Schedule = o.Schedule,
                    OrderedPs = o.OrderedPs,
                    Fee = o.Fee,
                    Status = o.Status,
                    PromoCode = o.PromoCode ?? string.Empty,
                    UserId = o.UserId,
                    RatingId = o.RatingId,
                    ReportId = o.ReportId,
                    Quantity = o.Quantity,
                    ModeOfPayment = o.ModeOfPayment,
                }).ToListAsync();

            foreach (var order in orders)
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

            return View(orders);
        }

        private Promo GetPromo(string code)
        {
            return _context.Promos.FirstOrDefault(p => p.PromoCode == code && p.PromoEnd > DateTime.Now);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptOrder(int clientId)
        {
            var order = await _context.ClientOrders
                .FirstOrDefaultAsync(o => o.ClientId == clientId && o.Status == false);

            if (order == null)
            {
                // Handle case where order does not exist
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
            order.Status = true;
            order.Fee = totalFee;

            // Log the accepted order into OrdersLog
            var orderLog = new OrdersLog
            {
                LogId = Guid.NewGuid().ToString(),
                OrderId = order.ClientId.ToString(),
                OrderDate = order.OrderDate,
                FirstName = order.FirstName,
                LastName = order.LastName,
                BusinessId = order.BusinessId,
                Item = order.OrderedPs,
                Qty = order.Quantity,
                Date = DateTime.Now,
                Status = "Accepted",
                Fee = totalFee,
                PromoCode = order.PromoCode
            };

            // Add the log to the OrdersLog table
            _context.OrdersLogs.Add(orderLog);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Orders));
        }

        public async Task<IActionResult> OrdersLog()
        {
            var logs = await _context.OrdersLogs
                .Select(log => new OrdersLogVM
                {
                    LogId = log.LogId,
                    OrderId = log.OrderId,
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

        public async Task<IActionResult> RefundRequests()
        {
            // Fetch refund requests from the logs
            var refundRequests = await _context.OrdersLogs
                .Where(log => log.Status == "Refund Requested")
                .Select(log => new RefundRequestVM
                {
                    LogId = log.LogId,
                    OrderId = log.OrderId,
                    OrderDate = log.OrderDate,
                    FirstName = log.FirstName,
                    LastName = log.LastName,
                    BusinessId = log.BusinessId,
                    Item = log.Item,
                    Qty = log.Qty,
                    Date = log.Date,
                    Status = log.Status,
                }).ToListAsync();

            foreach (var request in refundRequests)
            {
                if (!string.IsNullOrEmpty(request.PromoCode))
                {
                    // Calculate the original fee and discount amount
                    var (originalFee, discountAmount) = CalculateDiscount(request.PromoCode, request.Fee);

                    // Assign the calculated values
                    request.OriginalFee = originalFee;
                    request.DiscountAmount = discountAmount;
                    request.DiscountedFee = request.Fee; // This is already the discounted fee from the database

                    var promo = _context.Promos.FirstOrDefault(p => p.PromoCode == request.PromoCode);
                    request.DiscountPercentage = promo != null ? promo.Discount * 100 : 0; // Assign discount percentage if promo exists
                }
                else
                {
                    // If no promo, the discounted fee is the original fee
                    request.OriginalFee = request.Fee;
                    request.DiscountedFee = request.Fee;
                    request.DiscountPercentage = 0;
                    request.DiscountAmount = 0;
                }
            }
            return View(refundRequests);
        }

        public async Task<IActionResult> ShowRefundRequests(string businessId)
        {
            businessId = "5";  

            // Get the list of refund requests that belong to this provider's BusinessId
            var refundList = await _context.RefundRequests
                .Where(r => r.BusinessId == businessId)
                .ToListAsync();

            // Check if refundList is empty and add a message
            if (refundList.Count == 0)
            {
                ViewBag.NoRefundRequests = "No refund requests found for your business.";
            }

            return View(refundList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRefund(int refundId)
        {
            // Find the refund request by its RefundId
            var refundRequest = await _context.RefundRequests
                .FirstOrDefaultAsync(r => r.RefundId == refundId);

            // Check if the refund request exists
            if (refundRequest == null)
            {
                TempData["ErrorMessage"] = "Refund request not found.";
                return RedirectToAction("ShowRefundRequests");
            }

            // Reject the refund request (update status to "Rejected")
            refundRequest.RefundStatus = "Rejected";
            refundRequest.RefundActionDate = DateTime.Now;  

            // Optionally, you can add a rejection note if needed
            // refundRequest.RejectionNote = "The refund was rejected due to ...";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Refund request has been rejected successfully.";
            return RedirectToAction("ShowRefundRequests");
        }
    }
}
