using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Specifiactions;
using Talabat.Core.Services;
using Talabat.Core.Specifiactions.Order_Spec;
using Talabat.Repository.Data;
using Product = Talabat.Core.Entities.Product;

namespace Talabat.APIs.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : ApiBaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly StoreContext _storeContext;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly IResponseCacheService _cacheService;

        private const string ProductImagesFolder = "images/products";
        private const string PlaceholderImage = "images/products/placeholder.png";
        private const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public AdminController(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<AppUser> userManager,
            StoreContext storeContext,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IResponseCacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _storeContext = storeContext;
            _configuration = configuration;
            _env = env;
            _cacheService = cacheService;
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Dashboard Stats â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>GET api/admin/stats â†’ total counts</summary>
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetStats()
        {
            var totalProducts = await _storeContext.products.CountAsync();
            var totalUsers    = await _userManager.Users.CountAsync();
            var totalOrders   = await _storeContext.orders.CountAsync();

            return Ok(new
            {
                totalProducts,
                totalUsers,
                totalOrders
            });
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Product CRUD â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>
        /// POST api/admin/products/upload-image â†’ upload a product image from the admin's computer.
        /// Saves it under wwwroot/images/products and returns the relative path to store in Product.PictureUrl.
        /// </summary>
        [HttpPost("products/upload-image")]
        [RequestSizeLimit(MaxImageSizeBytes + 1024 * 1024)]
        public async Task<ActionResult<object>> UploadProductImage(IFormFile file)
        {
            if (file is null || file.Length == 0)
                return BadRequest(new ApiResponse(400, "No file was uploaded"));

            if (file.Length > MaxImageSizeBytes)
                return BadRequest(new ApiResponse(400, "Image is too large (max 5 MB)"));

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
                return BadRequest(new ApiResponse(400, "Only JPG, PNG, WEBP or GIF images are allowed"));

            // Make sure the content really is an image (not just a renamed file)
            if (!await HasValidImageSignatureAsync(file))
                return BadRequest(new ApiResponse(400, "The file is not a valid image"));

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var folder  = Path.Combine(webRoot, "images", "products");
            Directory.CreateDirectory(folder);

            // Random file name â†’ no overwriting, no path tricks from the original name
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(folder, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { pictureUrl = $"{ProductImagesFolder}/{fileName}" });
        }

        /// <summary>Checks the first bytes of the file for a JPG / PNG / GIF / WEBP signature.</summary>
        private static async Task<bool> HasValidImageSignatureAsync(IFormFile file)
        {
            var header = new byte[12];
            await using var stream = file.OpenReadStream();
            var read = await stream.ReadAsync(header, 0, header.Length);
            if (read < 4) return false;

            bool isPng  = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
            bool isJpg  = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            bool isGif  = header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38;
            bool isWebp = read >= 12
                          && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46   // RIFF
                          && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50; // WEBP
            return isPng || isJpg || isGif || isWebp;
        }

        /// <summary>
        /// The API prepends ApiBaseUrl when returning PictureUrl, so only the relative path may be stored.
        /// The edit form loads the already-resolved (full) URL, so strip our own base URL if present.
        /// </summary>
        private string NormalizePictureUrl(string pictureUrl)
        {
            if (string.IsNullOrWhiteSpace(pictureUrl)) return null;

            var baseUrl = _configuration["ApiBaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl) && pictureUrl.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase))
                return pictureUrl.Substring(baseUrl.Length);

            return pictureUrl;
        }

        /// <summary>Product list responses are cached in Redis; clear them so admin changes show immediately.</summary>
        private async Task InvalidateProductCacheAsync()
        {
            await _cacheService.RemoveCacheByPatternAsync("*products*");
            await _cacheService.RemoveCacheByPatternAsync("*Products*");
        }

        /// <summary>POST api/admin/products → create a product</summary>
        [HttpPost("products")]
        public async Task<ActionResult<ProductToReturnDto>> CreateProduct(AdminProductDto dto)
        {
            var product = new Product
            {
                Name           = dto.Name,
                Description    = dto.Description,
                Price          = dto.Price,
                PictureUrl     = NormalizePictureUrl(dto.PictureUrl) ?? PlaceholderImage,
                ProductBrandId = dto.ProductBrandId,
                ProductTypeId  = dto.ProductTypeId
            };

            await _unitOfWork.Repository<Product>().Add(product);
            var result = await _unitOfWork.Complete();

            if (result <= 0) return BadRequest(new ApiResponse(400, "Problem creating product"));

            await InvalidateProductCacheAsync();

            await _storeContext.Entry(product).Reference(p => p.ProductBrand).LoadAsync();
            await _storeContext.Entry(product).Reference(p => p.ProductType).LoadAsync();
            return Ok(_mapper.Map<Product, ProductToReturnDto>(product));
        }

        /// <summary>PUT api/admin/products/{id} → update a product</summary>
        [HttpPut("products/{id}")]
        public async Task<ActionResult<ProductToReturnDto>> UpdateProduct(int id, AdminProductDto dto)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);

            if (product is null) return NotFound(new ApiResponse(404, "Product not found"));

            product.Name           = dto.Name;
            product.Description    = dto.Description;
            product.Price          = dto.Price;
            // Removing the image (empty value) falls back to the placeholder instead of keeping/breaking the old one
            product.PictureUrl     = NormalizePictureUrl(dto.PictureUrl) ?? PlaceholderImage;
            product.ProductBrandId = dto.ProductBrandId;
            product.ProductTypeId  = dto.ProductTypeId;

            _unitOfWork.Repository<Product>().Update(product);
            var result = await _unitOfWork.Complete();

            if (result < 0) return BadRequest(new ApiResponse(400, "Problem updating product"));

            await InvalidateProductCacheAsync();

            await _storeContext.Entry(product).Reference(p => p.ProductBrand).LoadAsync();
            await _storeContext.Entry(product).Reference(p => p.ProductType).LoadAsync();

            return Ok(_mapper.Map<Product, ProductToReturnDto>(product));
        }

        /// <summary>POST api/admin/products/restore-seed → restores any missing default products</summary>
        [HttpPost("products/restore-seed")]
        public async Task<ActionResult> RestoreSeedProducts()
        {
            await StoreContextDataSeed.SeedAsync(_storeContext);
            await InvalidateProductCacheAsync();
            return Ok(new { message = "Seed products restored" });
        }

        /// <summary>DELETE api/admin/products/{id} → delete a product</summary>
        [HttpDelete("products/{id}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
                if (product is null) return NotFound(new ApiResponse(404, "Product not found"));

                // If the product had a custom uploaded file, clean up from wwwroot
                if (!string.IsNullOrEmpty(product.PictureUrl) &&
                    !product.PictureUrl.EndsWith("placeholder.png", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        var relPath = product.PictureUrl.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
                        var fullPath = Path.Combine(webRoot, relPath);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DeleteProduct] Could not remove file {product.PictureUrl}: {ex.Message}");
                    }
                }

                _unitOfWork.Repository<Product>().Delete(product);
                var result = await _unitOfWork.Complete();

                if (result <= 0) return BadRequest(new ApiResponse(400, "Problem deleting product"));

                await InvalidateProductCacheAsync();

                return Ok(new { message = "Product deleted successfully", id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting product {id}: {ex.Message}");
                return StatusCode(500, new ApiResponse(500, $"Problem deleting product: {ex.Message}"));
            }
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ User Management â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>GET api/admin/users â†’ list all users with roles</summary>
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<AdminUserDto>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<AdminUserDto>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                result.Add(new AdminUserDto
                {
                    Id          = u.Id,
                    DisplayName = u.DisplayName,
                    Email       = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Role        = roles.FirstOrDefault() ?? "User"
                });
            }

            return Ok(result);
        }

        /// <summary>GET api/admin/users/{id} â†’ get one user</summary>
        [HttpGet("users/{id}")]
        public async Task<ActionResult<AdminUserDto>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound(new ApiResponse(404, "User not found"));

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new AdminUserDto
            {
                Id          = user.Id,
                DisplayName = user.DisplayName,
                Email       = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role        = roles.FirstOrDefault() ?? "User"
            });
        }

        /// <summary>DELETE api/admin/users/{id} â†’ delete a user</summary>
        [HttpDelete("users/{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null) return NotFound(new ApiResponse(404, "User not found"));

            // Prevent deleting the admin itself
            if (user.Email == "admin@talabat.com")
                return BadRequest(new ApiResponse(400, "Cannot delete the super admin"));

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(new ApiResponse(400, "Problem deleting user"));

            return Ok(new { message = "User deleted successfully" });
        }

        // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€ Order Management â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        /// <summary>GET api/admin/orders â†’ list all orders across all users</summary>
        [HttpGet("orders")]
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetAllOrders()
        {
            var spec = new OrderSpecifications();
            var orders = await _unitOfWork.Repository<Order>().GetAllWithSpecAsync(spec);
            var mappedOrders = _mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDto>>(orders);
            return Ok(mappedOrders);
        }

        /// <summary>GET api/admin/orders/{id} â†’ get single order details</summary>
        [HttpGet("orders/{id}")]
        public async Task<ActionResult<OrderToReturnDto>> GetOrderById(int id)
        {
            var spec = new OrderSpecifications(id);
            var order = await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
            if (order is null) return NotFound(new ApiResponse(404, $"Order with id {id} not found"));

            var mappedOrder = _mapper.Map<Order, OrderToReturnDto>(order);
            return Ok(mappedOrder);
        }

        /// <summary>PUT api/admin/orders/{id}/status â†’ update order status</summary>
        [HttpPut("orders/{id}/status")]
        public async Task<ActionResult<OrderToReturnDto>> UpdateOrderStatus(int id, UpdateOrderStatusDto dto)
        {
            var order = await _storeContext.orders
                .Include(o => o.DeliveryMethod)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return NotFound(new ApiResponse(404, $"Order with id {id} not found"));

            var normalized = dto.Status?.Trim() ?? "";
            OrderStatus newStatus;

            if (Enum.TryParse<OrderStatus>(normalized, true, out var parsed))
            {
                newStatus = parsed;
            }
            else if (normalized.Equals("Payment Received", StringComparison.OrdinalIgnoreCase) ||
                     normalized.Equals("Payment Recieved", StringComparison.OrdinalIgnoreCase))
            {
                newStatus = OrderStatus.PaymentRecieved;
            }
            else if (normalized.Equals("Payment Failed", StringComparison.OrdinalIgnoreCase) ||
                     normalized.Equals("Payment Field", StringComparison.OrdinalIgnoreCase))
            {
                newStatus = OrderStatus.PaymentField;
            }
            else
            {
                return BadRequest(new ApiResponse(400, "Invalid order status value"));
            }

            if (order.Status != newStatus)
            {
                // Guard: if marking as PaymentRecieved without manual override, verify with Stripe first
                if (newStatus == OrderStatus.PaymentRecieved && !dto.ForceManualOverride && !string.IsNullOrWhiteSpace(order.PaymentIntentId))
                {
                    try
                    {
                        StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
                        var piService = new PaymentIntentService();
                        var paymentIntent = await piService.GetAsync(order.PaymentIntentId);
                        if (!string.Equals(paymentIntent?.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
                        {
                            return BadRequest(new ApiResponse(400,
                                $"Stripe reports PaymentIntent ({order.PaymentIntentId}) status is '{paymentIntent?.Status ?? "unknown"}' (not paid). Confirm Manual Override if you still want to mark it as Payment Received."));
                        }
                    }
                    catch (StripeException ex)
                    {
                        return BadRequest(new ApiResponse(400,
                            $"Could not verify PaymentIntent with Stripe ({ex.Message}). Confirm Manual Override to force status change."));
                    }
                }

                order.Status = newStatus;
                var result = await _storeContext.SaveChangesAsync();
                if (result <= 0) return BadRequest(new ApiResponse(400, "Problem updating order status"));
            }

            return Ok(_mapper.Map<Order, OrderToReturnDto>(order));
        }

        /// <summary>POST api/admin/orders/{id}/verify-payment â†’ queries Stripe live for this order's PaymentIntentId and syncs status</summary>
        [HttpPost("orders/{id}/verify-payment")]
        public async Task<ActionResult<StripeVerificationDto>> VerifyOrderPaymentWithStripe(int id)
        {
            var order = await _storeContext.orders
                .Include(o => o.DeliveryMethod)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return NotFound(new ApiResponse(404, $"Order with id {id} not found"));

            if (string.IsNullOrWhiteSpace(order.PaymentIntentId))
            {
                return Ok(new StripeVerificationDto
                {
                    OrderId = order.Id,
                    PaymentIntentId = "",
                    StripeStatus = "no_payment_intent",
                    IsVerifiedPaid = false,
                    Amount = 0,
                    Currency = "USD",
                    ErrorMessage = "This order does not have a Stripe PaymentIntent ID.",
                    Order = _mapper.Map<Order, OrderToReturnDto>(order)
                });
            }

            try
            {
                StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(order.PaymentIntentId);

                var isPaid = string.Equals(paymentIntent.Status, "succeeded", StringComparison.OrdinalIgnoreCase);
                OrderStatus syncedStatus = order.Status;

                if (isPaid)
                {
                    syncedStatus = OrderStatus.PaymentRecieved;
                }
                else if (string.Equals(paymentIntent.Status, "canceled", StringComparison.OrdinalIgnoreCase) ||
                         paymentIntent.LastPaymentError != null)
                {
                    syncedStatus = OrderStatus.PaymentField;
                }

                if (order.Status != syncedStatus)
                {
                    order.Status = syncedStatus;
                    await _storeContext.SaveChangesAsync();
                }

                return Ok(new StripeVerificationDto
                {
                    OrderId = order.Id,
                    PaymentIntentId = paymentIntent.Id,
                    StripeStatus = paymentIntent.Status,
                    IsVerifiedPaid = isPaid,
                    Amount = paymentIntent.Amount / 100m,
                    Currency = (paymentIntent.Currency ?? "usd").ToUpperInvariant(),
                    ErrorMessage = paymentIntent.LastPaymentError?.Message,
                    Order = _mapper.Map<Order, OrderToReturnDto>(order)
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new ApiResponse(400, $"Stripe verification failed: {ex.Message}"));
            }
        }

        /// <summary>POST api/admin/orders/verify-all-pending â†’ checks all Pending orders with Stripe and updates any that succeeded/failed</summary>
        [HttpPost("orders/verify-all-pending")]
        public async Task<ActionResult<object>> VerifyAllPendingOrdersWithStripe()
        {
            var pendingOrders = await _storeContext.orders
                .Where(o => o.Status == OrderStatus.Pending && !string.IsNullOrEmpty(o.PaymentIntentId))
                .ToListAsync();

            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
            var service = new PaymentIntentService();
            int updatedCount = 0;

            foreach (var order in pendingOrders)
            {
                try
                {
                    var pi = await service.GetAsync(order.PaymentIntentId);
                    if (string.Equals(pi?.Status, "succeeded", StringComparison.OrdinalIgnoreCase))
                    {
                        order.Status = OrderStatus.PaymentRecieved;
                        updatedCount++;
                    }
                    else if (string.Equals(pi?.Status, "canceled", StringComparison.OrdinalIgnoreCase) ||
                             pi?.LastPaymentError != null)
                    {
                        order.Status = OrderStatus.PaymentField;
                        updatedCount++;
                    }
                }
                catch
                {
                    // Skip orders whose PaymentIntent cannot be retrieved
                }
            }

            if (updatedCount > 0)
            {
                await _storeContext.SaveChangesAsync();
            }

            return Ok(new
            {
                checkedCount = pendingOrders.Count,
                updatedCount,
                message = $"Checked {pendingOrders.Count} pending order(s) with Stripe; {updatedCount} order(s) updated."
            });
        }

        /// <summary>DELETE api/admin/orders/{id} â†’ delete an order</summary>
        [HttpDelete("orders/{id}")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            var order = await _storeContext.orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order is null) return NotFound(new ApiResponse(404, $"Order with id {id} not found"));

            if (order.Items != null && order.Items.Any())
            {
                _storeContext.orderItems.RemoveRange(order.Items);
            }

            _storeContext.orders.Remove(order);
            var result = await _storeContext.SaveChangesAsync();

            if (result <= 0) return BadRequest(new ApiResponse(400, "Problem deleting order"));

            return Ok(new { message = "Order deleted successfully" });
        }
    }
}
