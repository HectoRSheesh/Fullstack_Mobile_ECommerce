// Sipariş yönetimi için controller
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using dotnet_core_webservice.Data;
using dotnet_core_webservice.DTOs;
using dotnet_core_webservice.Models;
using System.Security.Claims;

namespace dotnet_core_webservice.Controllers
{
    /// <summary>
    /// Sipariş yönetimi API Controller
    /// Sipariş oluşturma, listeleme, detay görüntüleme işlemleri
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Sipariş işlemleri için login gerekli
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Test endpoint - Order API çalışıp çalışmadığını kontrol eder
        /// GET /api/order/test
        /// </summary>
        [HttpGet("test")]
        [AllowAnonymous] // Test için anonymous erişim
        public IActionResult Test()
        {
            return Ok(new { 
                message = "Order API is working!", 
                timestamp = DateTime.Now,
                database_connected = _context.Database.CanConnect()
            });
        }

        /// <summary>
        /// JWT token'dan kullanıcı ID'sini alır
        /// </summary>
        /// <returns>Kullanıcı ID'si</returns>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        /// <summary>
        /// Kullanıcının tüm siparişlerini getirir
        /// GET /api/order
        /// </summary>
        /// <returns>Sipariş listesi</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<OrderDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                Console.WriteLine("GetOrders endpoint called");
                
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    Console.WriteLine("GetOrders: User ID is null - unauthorized");
                    return Unauthorized(new { message = "Invalid user token" });
                }

                Console.WriteLine($"GetOrders: Getting orders for user ID: {userId.Value}");

                var orders = await _context.Orders
                    .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                    .Where(o => o.UserId == userId.Value)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                Console.WriteLine($"GetOrders: Found {orders.Count} orders");

                var orderDtos = orders.Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status.ToString(),
                    TotalAmount = o.TotalAmount,
                    ShippingCost = o.ShippingCost,
                    TaxAmount = o.TaxAmount,
                    ShippingAddress = o.ShippingAddress,
                    ShippingCity = o.ShippingCity,
                    PaymentMethod = o.PaymentMethod,
                    TotalItems = o.OrderItems?.Sum(oi => oi.Quantity) ?? 0,
                    Items = o.OrderItems?.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList() ?? new List<OrderItemDto>(),
                    CanBeCancelled = o.Status == OrderStatus.Pending
                }).ToList();

                return Ok(orderDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetOrders error: {ex.Message}");
                return BadRequest(new { message = "Failed to retrieve orders", error = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir siparişin detaylarını getirir
        /// GET /api/order/{id}
        /// </summary>
        /// <param name="id">Sipariş ID'si</param>
        /// <returns>Sipariş detayları</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                Console.WriteLine($"GetOrderById endpoint called for ID: {id}");
                
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    Console.WriteLine("GetOrderById: User ID is null - unauthorized");
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var order = await _context.Orders
                    .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId.Value);

                if (order == null)
                {
                    Console.WriteLine($"GetOrderById: Order not found for ID: {id}");
                    return NotFound(new { message = "Order not found" });
                }

                var orderDto = new OrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.OrderDate,
                    Status = order.Status.ToString(),
                    TotalAmount = order.TotalAmount,
                    ShippingCost = order.ShippingCost,
                    TaxAmount = order.TaxAmount,
                    ShippingAddress = order.ShippingAddress,
                    ShippingCity = order.ShippingCity,
                    PaymentMethod = order.PaymentMethod,
                    TotalItems = order.OrderItems?.Sum(oi => oi.Quantity) ?? 0,
                    Items = order.OrderItems?.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList() ?? new List<OrderItemDto>(),
                    CanBeCancelled = order.Status == OrderStatus.Pending
                };

                Console.WriteLine($"GetOrderById: Returning order {order.OrderNumber}");
                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetOrderById error: {ex.Message}");
                return BadRequest(new { message = "Failed to retrieve order", error = ex.Message });
            }
        }

        /// <summary>
        /// Sepetteki ürünlerden sipariş oluşturur
        /// POST /api/order/create-from-cart
        /// </summary>
        /// <param name="checkoutDto">Checkout bilgileri</param>
        /// <returns>Oluşturulan sipariş bilgisi</returns>
        [HttpPost("create-from-cart")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateOrderFromCart([FromBody] CheckoutDto checkoutDto)
        {
            try
            {
                Console.WriteLine("CreateOrderFromCart endpoint called");
                Console.WriteLine($"CreateOrderFromCart: Received data - ShippingAddress: {checkoutDto?.ShippingAddress}");
                
                if (checkoutDto == null)
                {
                    Console.WriteLine("CreateOrderFromCart: CheckoutDto is null");
                    return BadRequest(new { message = "Invalid checkout data" });
                }

                if (string.IsNullOrEmpty(checkoutDto.ShippingAddress))
                {
                    Console.WriteLine("CreateOrderFromCart: ShippingAddress is null or empty");
                    return BadRequest(new { message = "Shipping address is required" });
                }
                
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    Console.WriteLine("CreateOrderFromCart: User ID is null - unauthorized");
                    return Unauthorized(new { message = "Invalid user token" });
                }

                Console.WriteLine($"CreateOrderFromCart: Processing order for user ID: {userId.Value}");

                // Sepetteki ürünleri al
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Where(ci => ci.UserId == userId.Value)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    Console.WriteLine("CreateOrderFromCart: Cart is empty");
                    return BadRequest(new { message = "Cart is empty" });
                }

                Console.WriteLine($"CreateOrderFromCart: Found {cartItems.Count} items in cart");

                // Stok kontrolü
                foreach (var cartItem in cartItems)
                {
                    if (cartItem.Product == null || !cartItem.Product.IsActive)
                    {
                        Console.WriteLine($"CreateOrderFromCart: Product not found or inactive: {cartItem.ProductId}");
                        return BadRequest(new { message = $"Product {cartItem.ProductId} is not available" });
                    }

                    if (cartItem.Product.StockQuantity < cartItem.Quantity)
                    {
                        Console.WriteLine($"CreateOrderFromCart: Insufficient stock for product {cartItem.ProductId}. Required: {cartItem.Quantity}, Available: {cartItem.Product.StockQuantity}");
                        return BadRequest(new { message = $"Insufficient stock for {cartItem.Product.Name}" });
                    }
                }

                // Toplam tutarları hesapla
                var totalAmount = cartItems.Sum(ci => ci.Product!.Price * ci.Quantity);
                var shippingCost = CalculateShippingCost(totalAmount);
                var taxAmount = CalculateTax(totalAmount);
                var grandTotal = totalAmount + shippingCost + taxAmount;

                Console.WriteLine($"CreateOrderFromCart: Total amount: {totalAmount}, Shipping: {shippingCost}, Tax: {taxAmount}, Grand Total: {grandTotal}");

                // Sipariş oluştur
                var order = new Order
                {
                    UserId = userId.Value,
                    OrderNumber = GenerateOrderNumber(),
                    TotalAmount = grandTotal,
                    ShippingCost = shippingCost,
                    TaxAmount = taxAmount,
                    ShippingAddress = checkoutDto.ShippingAddress,
                    ShippingCity = checkoutDto.ShippingCity ?? "Not specified",
                    PaymentMethod = checkoutDto.PaymentMethod ?? "Cash on Delivery",
                    Status = OrderStatus.Pending
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Sipariş öğelerini oluştur
                var orderItems = cartItems.Select(ci => new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product!.Name,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product!.Price,
                    TotalPrice = ci.Product.Price * ci.Quantity
                }).ToList();

                _context.OrderItems.AddRange(orderItems);

                // Stok miktarlarını güncelle
                foreach (var cartItem in cartItems)
                {
                    cartItem.Product!.StockQuantity -= cartItem.Quantity;
                }

                // Sepeti temizle
                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();

                Console.WriteLine($"CreateOrderFromCart: Order created successfully with ID: {order.Id}");

                return Ok(new 
                { 
                    message = "Order created successfully",
                    orderId = order.Id,
                    orderNumber = order.OrderNumber,
                    totalAmount = grandTotal
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateOrderFromCart error: {ex.Message}");
                Console.WriteLine($"CreateOrderFromCart stack trace: {ex.StackTrace}");
                return BadRequest(new { message = "Failed to create order", error = ex.Message });
            }
        }

        /// <summary>
        /// Sipariş durumunu günceller (Admin için)
        /// PUT /api/order/{id}/status
        /// </summary>
        /// <param name="id">Sipariş ID'si</param>
        /// <param name="status">Yeni durum</param>
        /// <returns>Güncellenmiş sipariş</returns>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            try
            {
                Console.WriteLine($"UpdateOrderStatus endpoint called for order ID: {id}, new status: {status}");
                
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId.Value);
                if (order == null)
                {
                    return NotFound(new { message = "Order not found" });
                }

                if (Enum.TryParse<OrderStatus>(status, true, out OrderStatus newStatus))
                {
                    order.Status = newStatus;
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"UpdateOrderStatus: Order {id} status updated to {newStatus}");
                    return Ok(new { message = "Order status updated successfully", orderId = id, newStatus = newStatus.ToString() });
                }
                else
                {
                    return BadRequest(new { message = "Invalid order status" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateOrderStatus error: {ex.Message}");
                return BadRequest(new { message = "Failed to update order status", error = ex.Message });
            }
        }

        /// <summary>
        /// Siparişi iptal eder
        /// DELETE /api/order/{id}
        /// </summary>
        /// <param name="id">Sipariş ID'si</param>
        /// <returns>İptal mesajı</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                Console.WriteLine($"CancelOrder endpoint called for order ID: {id}");
                
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var order = await _context.Orders
                    .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId.Value);

                if (order == null)
                {
                    return NotFound(new { message = "Order not found" });
                }

                // Sadece beklemede olan siparişler iptal edilebilir
                if (order.Status != OrderStatus.Pending)
                {
                    return BadRequest(new { message = "Only pending orders can be cancelled" });
                }

                // Stok miktarlarını geri yükle
                if (order.OrderItems != null)
                {
                    foreach (var orderItem in order.OrderItems)
                    {
                        if (orderItem.Product != null)
                        {
                            orderItem.Product.StockQuantity += orderItem.Quantity;
                        }
                    }
                }

                // Sipariş durumunu iptal olarak güncelle
                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();

                Console.WriteLine($"CancelOrder: Order {id} cancelled successfully");
                return Ok(new { message = "Order cancelled successfully", orderId = id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CancelOrder error: {ex.Message}");
                return BadRequest(new { message = "Failed to cancel order", error = ex.Message });
            }
        }

        /// <summary>
        /// Kargo ücretini hesaplar
        /// </summary>
        /// <param name="totalAmount">Toplam tutar</param>
        /// <returns>Kargo ücreti</returns>
        private decimal CalculateShippingCost(decimal totalAmount)
        {
            // 200 TL üzeri siparişlerde kargo ücretsiz
            if (totalAmount >= 200)
                return 0;
            
            // Diğer durumlarda 15 TL kargo ücreti
            return 15;
        }

        /// <summary>
        /// Vergi tutarını hesaplar (%18 KDV)
        /// </summary>
        /// <param name="totalAmount">Toplam tutar</param>
        /// <returns>Vergi tutarı</returns>
        private decimal CalculateTax(decimal totalAmount)
        {
            return totalAmount * 0.18m; // %18 KDV
        }

        /// <summary>
        /// Benzersiz sipariş numarası oluşturur
        /// </summary>
        /// <returns>Sipariş numarası</returns>
        private string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyyy}-{DateTime.Now:MMdd}-{new Random().Next(1000, 9999)}";
        }
    }
}
