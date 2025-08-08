// Ürün yönetimi için controller
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
    /// Ürün yönetimi API Controller
    /// Ürün CRUD işlemleri, arama ve filtreleme endpoint'leri
    /// </summary>
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor - Dependency Injection
        /// </summary>
        /// <param name="context">Entity Framework context</param>
        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm ürünleri listeler (sayfalama ve filtreleme ile)
        /// GET /api/product
        /// </summary>
        /// <param name="searchDto">Arama ve filtreleme kriterleri</param>
        /// <returns>Sayfalanmış ürün listesi</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<ProductDto>), 200)]
        public async Task<IActionResult> GetProducts([FromQuery] ProductSearchDto searchDto)
        {
            try
            {
                Console.WriteLine("GetProducts endpoint called");
                
                var query = _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive)
                    .AsQueryable();

                // Total product count
                var totalProductCount = await _context.Products.CountAsync();
                var activeProductCount = await query.CountAsync();
                
                Console.WriteLine($"Total products in DB: {totalProductCount}");
                Console.WriteLine($"Active products: {activeProductCount}");

                // Arama terimi filtresi
                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    Console.WriteLine($"Search term: {searchDto.SearchTerm}");
                    query = query.Where(p => p.Name.Contains(searchDto.SearchTerm) || 
                                           (p.Description != null && p.Description.Contains(searchDto.SearchTerm)));
                }

            // Kategori filtresi (alt kategoriler dahil)
            if (searchDto.CategoryId.HasValue)
            {
                // Seçilen kategori ve alt kategorilerini bulur
                var categoryIds = await _context.Categories
                    .Where(c => c.Id == searchDto.CategoryId.Value || c.ParentCategoryId == searchDto.CategoryId.Value)
                    .Select(c => c.Id)
                    .ToListAsync();
                
                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            // Fiyat aralığı filtresi
            if (searchDto.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= searchDto.MinPrice.Value);
            }

            if (searchDto.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= searchDto.MaxPrice.Value);
            }

            // Stok durumu filtresi
            if (searchDto.InStockOnly == true)
            {
                query = query.Where(p => p.StockQuantity > 0);
            }

            // Sıralama
            query = searchDto.SortBy?.ToLower() switch
            {
                "price" => searchDto.SortOrder?.ToLower() == "desc" 
                    ? query.OrderByDescending(p => p.Price) 
                    : query.OrderBy(p => p.Price),
                "date" => searchDto.SortOrder?.ToLower() == "desc" 
                    ? query.OrderByDescending(p => p.CreatedAt) 
                    : query.OrderBy(p => p.CreatedAt),
                _ => searchDto.SortOrder?.ToLower() == "desc" 
                    ? query.OrderByDescending(p => p.Name) 
                    : query.OrderBy(p => p.Name)
            };

            // Toplam kayıt sayısı
            var totalCount = await query.CountAsync();

            // Sayfalama
            var products = await query
                .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category!.Name,
                    ImageUrl = p.ImageUrl,
                    SKU = p.SKU,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            var result = new PagedResultDto<ProductDto>
            {
                Data = products,
                TotalCount = totalCount,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize
            };

            Console.WriteLine($"Returning {products.Count} products");
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetProducts error: {ex.Message}");
            return BadRequest(new { message = "Failed to get products", error = ex.Message });
        }
    }

        /// <summary>
        /// Belirli bir ürünü getirir
        /// GET /api/product/{id}
        /// </summary>
        /// <param name="id">Ürün ID'si</param>
        /// <returns>Ürün detayları</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id && p.IsActive)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category!.Name,
                    ImageUrl = p.ImageUrl,
                    SKU = p.SKU,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            return Ok(product);
        }

        /// <summary>
        /// Yeni ürün oluşturur (Admin/Manager yetkisi gerekli)
        /// POST /api/product
        /// </summary>
        /// <param name="createProductDto">Ürün oluşturma bilgileri</param>
        /// <returns>Oluşturulan ürün</returns>
        [HttpPost]
        [Authorize] // JWT token gerekli
        [ProducesResponseType(typeof(ProductDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kullanıcı ID'sini JWT token'dan al
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Kategori kontrolü
            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == createProductDto.CategoryId && c.IsActive);
            if (!categoryExists)
            {
                return BadRequest(new { message = "Category not found" });
            }

            // SKU benzersizlik kontrolü
            if (!string.IsNullOrEmpty(createProductDto.SKU))
            {
                var skuExists = await _context.Products.AnyAsync(p => p.SKU == createProductDto.SKU);
                if (skuExists)
                {
                    return BadRequest(new { message = "SKU already exists" });
                }
            }

            // Yeni ürün oluştur
            var product = new Product
            {
                Name = createProductDto.Name,
                Description = createProductDto.Description,
                Price = createProductDto.Price,
                StockQuantity = createProductDto.StockQuantity,
                CategoryId = createProductDto.CategoryId,
                ImageUrl = createProductDto.ImageUrl,
                SKU = createProductDto.SKU,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Oluşturulan ürünü kategori bilgisi ile birlikte döndür
            var createdProduct = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == product.Id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category!.Name,
                    ImageUrl = p.ImageUrl,
                    SKU = p.SKU,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .FirstAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, createdProduct);
        }

        /// <summary>
        /// Ürün günceller (Admin/Manager veya ürün sahibi)
        /// PUT /api/product/{id}
        /// </summary>
        /// <param name="id">Güncellenecek ürün ID'si</param>
        /// <param name="updateProductDto">Güncelleme bilgileri</param>
        /// <returns>Güncellenmiş ürün</returns>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null || !product.IsActive)
            {
                return NotFound(new { message = "Product not found" });
            }

            // Kullanıcı yetkisi kontrolü (gelecekte role-based auth eklenebilir)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user token" });
            }

            // Güncelleme işlemleri (sadece gönderilen alanları güncelle)
            if (!string.IsNullOrEmpty(updateProductDto.Name))
                product.Name = updateProductDto.Name;

            if (updateProductDto.Description != null)
                product.Description = updateProductDto.Description;

            if (updateProductDto.Price.HasValue)
                product.Price = updateProductDto.Price.Value;

            if (updateProductDto.StockQuantity.HasValue)
                product.StockQuantity = updateProductDto.StockQuantity.Value;

            if (updateProductDto.CategoryId.HasValue)
            {
                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == updateProductDto.CategoryId.Value && c.IsActive);
                if (!categoryExists)
                {
                    return BadRequest(new { message = "Category not found" });
                }
                product.CategoryId = updateProductDto.CategoryId.Value;
            }

            if (updateProductDto.ImageUrl != null)
                product.ImageUrl = updateProductDto.ImageUrl;

            if (updateProductDto.IsActive.HasValue)
                product.IsActive = updateProductDto.IsActive.Value;

            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Güncellenmiş ürünü döndür
            var updatedProduct = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category!.Name,
                    ImageUrl = p.ImageUrl,
                    SKU = p.SKU,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt
                })
                .FirstAsync();

            return Ok(updatedProduct);
        }

        /// <summary>
        /// Ürünü siler (soft delete)
        /// DELETE /api/product/{id}
        /// </summary>
        /// <param name="id">Silinecek ürün ID'si</param>
        /// <returns>Silme işlemi sonucu</returns>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            // Soft delete - ürünü deaktive et
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully" });
        }

        /// <summary>
        /// Kategoriye göre ürünleri listeler
        /// GET /api/product/category/{categoryId}
        /// </summary>
        /// <param name="categoryId">Kategori ID'si</param>
        /// <param name="pageNumber">Sayfa numarası</param>
        /// <param name="pageSize">Sayfa boyutu</param>
        /// <returns>Kategorideki ürünler</returns>
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(typeof(PagedResultDto<ProductDto>), 200)]
        public async Task<IActionResult> GetProductsByCategory(int categoryId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var searchDto = new ProductSearchDto
            {
                CategoryId = categoryId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return await GetProducts(searchDto);
        }

        /// <summary>
        /// Test endpoint - API çalışıp çalışmadığını kontrol eder
        /// GET /api/product/test
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { 
                message = "Product API is working!", 
                timestamp = DateTime.Now,
                database_connected = _context.Database.CanConnect()
            });
        }
    }
}
