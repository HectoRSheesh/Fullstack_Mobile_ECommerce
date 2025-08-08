// Alışveriş sepeti yönetimi için controller
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
    /// Alışveriş sepeti yönetimi API Controller
    /// Sepete ürün ekleme, çıkarma, güncelleme işlemleri
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Sepet işlemleri için login gerekli
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor - Dependency Injection
        /// </summary>
        /// <param name="context">Entity Framework context</param>
        public CartController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Kullanıcının sepetini getirir
        /// GET /api/cart
        /// </summary>
        /// <returns>Sepet özeti</returns>
        [HttpGet]
        [ProducesResponseType(typeof(CartSummaryDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                Console.WriteLine("GetCart endpoint called");
                
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    Console.WriteLine("GetCart: User ID is null");
                    return Unauthorized(new { message = "Invalid user token" });
                }

                Console.WriteLine($"GetCart: Getting cart for user ID: {userId.Value}");

                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Where(ci => ci.UserId == userId.Value)
                    .Select(ci => new CartItemDto
                    {
                        Id = ci.Id,
                        ProductId = ci.ProductId,
                        ProductName = ci.Product!.Name,
                        ProductPrice = ci.Product.Price,
                        ProductImageUrl = ci.Product.ImageUrl,
                        Quantity = ci.Quantity,
                        IsAvailable = ci.Product.IsActive && ci.Product.StockQuantity >= ci.Quantity,
                        AvailableStock = ci.Product.StockQuantity
                    })
                    .ToListAsync();

                Console.WriteLine($"GetCart: Found {cartItems.Count} items in cart");
                foreach (var item in cartItems)
                {
                    Console.WriteLine($"Cart Item: ID={item.Id}, Product={item.ProductName}, Qty={item.Quantity}, Price={item.ProductPrice}");
                }

                var cartSummary = new CartSummaryDto
                {
                    Items = cartItems,
                    ShippingCost = CalculateShippingCost(cartItems.Sum(i => i.TotalPrice)),
                    TaxAmount = CalculateTax(cartItems.Sum(i => i.TotalPrice))
                };

                Console.WriteLine($"GetCart: Returning cart summary with {cartSummary.Items.Count} items, TotalAmount={cartSummary.TotalAmount}");
                return Ok(cartSummary);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCart error: {ex.Message}");
                Console.WriteLine($"GetCart stack trace: {ex.StackTrace}");
                return BadRequest(new { message = "Failed to get cart", error = ex.Message, details = ex.ToString() });
            }
        }

        /// <summary>
        /// Sepete ürün ekler
        /// POST /api/cart/add
        /// </summary>
        /// <param name="addToCartDto">Sepete ekleme bilgileri</param>
        /// <returns>Güncellenmiş sepet</returns>
        [HttpPost("add")]
        [ProducesResponseType(typeof(CartSummaryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto addToCartDto)
        {
            try
            {
                Console.WriteLine("AddToCart endpoint called");
                Console.WriteLine($"ProductId: {addToCartDto?.ProductId}, Quantity: {addToCartDto?.Quantity}");
                
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("AddToCart: Model state is invalid");
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    Console.WriteLine("AddToCart: User ID is null - unauthorized");
                    return Unauthorized(new { message = "Invalid user token" });
                }

                Console.WriteLine($"AddToCart: Adding to cart for user ID: {userId.Value}");

                // Önce DTO'nun null olmadığını kontrol et
                if (addToCartDto == null)
                {
                    Console.WriteLine("AddToCart: AddToCartDto is null");
                    return BadRequest(new { message = "Invalid request data" });
                }

                // DTO'nun geçerli değerlere sahip olduğunu kontrol et
                if (addToCartDto.ProductId <= 0 || addToCartDto.Quantity <= 0)
                {
                    Console.WriteLine("AddToCart: Invalid product ID or quantity");
                    return BadRequest(new { message = "Invalid product ID or quantity" });
                }

                // Ürün kontrolü
                var product = await _context.Products.FindAsync(addToCartDto.ProductId);
                if (product == null || !product.IsActive)
                {
                    Console.WriteLine($"AddToCart: Product not found or inactive: {addToCartDto.ProductId}");
                    return BadRequest(new { message = "Product not found or not available" });
                }

                Console.WriteLine($"AddToCart: Product found: {product.Name}, Stock: {product.StockQuantity}");

                // Stok kontrolü
                if (product.StockQuantity < addToCartDto.Quantity)
                {
                    Console.WriteLine($"AddToCart: Insufficient stock. Requested: {addToCartDto.Quantity}, Available: {product.StockQuantity}");
                    return BadRequest(new { message = "Insufficient stock" });
                }

                // Mevcut sepet ürününü kontrol et
                var existingCartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.UserId == userId.Value && ci.ProductId == addToCartDto.ProductId);

                if (existingCartItem != null)
                {
                    Console.WriteLine($"AddToCart: Updating existing cart item. Old quantity: {existingCartItem.Quantity}");
                    existingCartItem.Quantity += addToCartDto.Quantity;
                    existingCartItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    Console.WriteLine("AddToCart: Creating new cart item");
                    var newCartItem = new CartItem
                    {
                        UserId = userId.Value,
                        ProductId = addToCartDto.ProductId,
                        Quantity = addToCartDto.Quantity
                    };
                    _context.CartItems.Add(newCartItem);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("AddToCart: Cart updated successfully");

                // Güncellenmiş sepeti döndür
                return await GetCart();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddToCart error: {ex.Message}");
                Console.WriteLine($"AddToCart stack trace: {ex.StackTrace}");
                return BadRequest(new { message = "Failed to add to cart", error = ex.Message });
            }
        }

        /// <summary>
        /// Sepetteki ürün miktarını günceller
        /// PUT /api/cart/{id}
        /// </summary>
        /// <param name="id">Sepet öğesi ID'si</param>
        /// <param name="updateDto">Güncellenecek miktar</param>
        /// <returns>Güncellenmiş sepet</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CartSummaryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCartItem(int id, [FromBody] UpdateCartItemDto updateDto)
        {
            try
            {
                Console.WriteLine($"UpdateCartItem endpoint called for ID: {id}, Quantity: {updateDto?.Quantity}");
                
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("UpdateCartItem: Model state is invalid");
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    Console.WriteLine("UpdateCartItem: User ID is null - unauthorized");
                    return Unauthorized(new { message = "Invalid user token" });
                }

                Console.WriteLine($"UpdateCartItem: Updating cart item for user ID: {userId.Value}");

                // Sepet öğesini bul ve ürün bilgilerini dahil et
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Product)
                    .FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId.Value);

                if (cartItem == null)
                {
                    Console.WriteLine($"UpdateCartItem: Cart item not found: {id}");
                    return NotFound(new { message = "Cart item not found" });
                }

                // Stok kontrolü
                var quantity = updateDto?.Quantity ?? 0;
                if (cartItem.Product != null && quantity > cartItem.Product.StockQuantity)
                {
                    Console.WriteLine($"UpdateCartItem: Insufficient stock. Requested: {quantity}, Available: {cartItem.Product.StockQuantity}");
                    return BadRequest(new { message = "Insufficient stock", availableStock = cartItem.Product.StockQuantity });
                }

                Console.WriteLine($"UpdateCartItem: Found cart item. Old quantity: {cartItem.Quantity}, New quantity: {quantity}");

                // Miktarı güncelle
                cartItem.Quantity = quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                Console.WriteLine("UpdateCartItem: Cart item updated successfully");

                // Güncellenmiş sepeti döndür
                return await GetCart();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateCartItem error: {ex.Message}");
                Console.WriteLine($"UpdateCartItem stack trace: {ex.StackTrace}");
                return BadRequest(new { message = "Failed to update cart item", error = ex.Message });
            }
        }

        /// <summary>
        /// Sepetten ürün kaldırır
        /// DELETE /api/cart/{id}
        /// </summary>
        /// <param name="id">Sepet öğesi ID'si</param>
        /// <returns>Güncellenmiş sepet</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(CartSummaryDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveCartItem(int id)
        {
            try
            {
                Console.WriteLine($"RemoveCartItem endpoint called for ID: {id}");
                
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    Console.WriteLine("RemoveCartItem: User ID is null - unauthorized");
                    return Unauthorized(new { message = "Invalid user token" });
                }

                Console.WriteLine($"RemoveCartItem: Removing cart item for user ID: {userId.Value}");

                // Sepet öğesini bul
                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.Id == id && ci.UserId == userId.Value);

                if (cartItem == null)
                {
                    Console.WriteLine($"RemoveCartItem: Cart item not found: {id}");
                    return NotFound(new { message = "Cart item not found" });
                }

                Console.WriteLine($"RemoveCartItem: Found cart item. Removing...");

                // Sepet öğesini kaldır
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                
                Console.WriteLine("RemoveCartItem: Cart item removed successfully");

                // Güncellenmiş sepeti döndür
                return await GetCart();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RemoveCartItem error: {ex.Message}");
                Console.WriteLine($"RemoveCartItem stack trace: {ex.StackTrace}");
                return BadRequest(new { message = "Failed to remove cart item", error = ex.Message });
            }
        }

        /// <summary>
        /// Sepeti tamamen temizler
        /// DELETE /api/cart
        /// </summary>
        /// <returns>Başarı mesajı</returns>
        [HttpDelete]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                Console.WriteLine("ClearCart endpoint called");
                
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    Console.WriteLine("ClearCart: User ID is null - unauthorized");
                    return Unauthorized(new { message = "Invalid user token" });
                }

                Console.WriteLine($"ClearCart: Clearing cart for user ID: {userId.Value}");

                // Kullanıcının tüm sepet öğelerini bul ve kaldır
                var cartItems = await _context.CartItems
                    .Where(ci => ci.UserId == userId.Value)
                    .ToListAsync();

                Console.WriteLine($"ClearCart: Found {cartItems.Count} items to remove");

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
                
                Console.WriteLine("ClearCart: Cart cleared successfully");

                return Ok(new { message = "Cart cleared successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClearCart error: {ex.Message}");
                Console.WriteLine($"ClearCart stack trace: {ex.StackTrace}");
                return BadRequest(new { message = "Failed to clear cart", error = ex.Message });
            }
        }

        /// <summary>
        /// Sepetteki ürünlerden sipariş oluşturur
        /// POST /api/cart/checkout
        /// Not: Bu endpoint deprecated - bunun yerine /api/order/create-from-cart kullanın
        /// </summary>
        /// <param name="checkoutDto">Checkout bilgileri</param>
        /// <returns>Oluşturulan sipariş bilgisi</returns>
        [HttpPost("checkout")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [Obsolete("Bu endpoint deprecated - bunun yerine /api/order/create-from-cart kullanın")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutDto checkoutDto)
        {
            // Yeni OrderController'daki endpoint'e yönlendir
            var orderController = new OrderController(_context);
            return await orderController.CreateOrderFromCart(checkoutDto);
        }

        /// <summary>
        /// Test endpoint - Cart API çalışıp çalışmadığını kontrol eder
        /// GET /api/cart/test
        /// </summary>
        [HttpGet("test")]
        [AllowAnonymous] // Test için anonymous erişim
        public IActionResult Test()
        {
            return Ok(new { 
                message = "Cart API is working!", 
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
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user ID: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Kargo ücretini hesaplar
        /// </summary>
        /// <param name="cartTotal">Sepet toplamı</param>
        /// <returns>Kargo ücreti</returns>
        private decimal CalculateShippingCost(decimal cartTotal)
        {
            // 150 TL üzeri kargo ücretsiz
            return cartTotal >= 150 ? 0 : 15;
        }

        /// <summary>
        /// KDV miktarını hesaplar
        /// </summary>
        /// <param name="cartTotal">Sepet toplamı</param>
        /// <returns>KDV miktarı</returns>
        private decimal CalculateTax(decimal cartTotal)
        {
            // %18 KDV
            return cartTotal * 0.18m;
        }

        /// <summary>
        /// Benzersiz sipariş numarası oluşturur
        /// </summary>
        /// <returns>Sipariş numarası</returns>
        private string GenerateOrderNumber()
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return $"ORD-{year}{month:D2}-{timestamp}";
        }
    }
}
