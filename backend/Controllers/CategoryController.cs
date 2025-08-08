// Kategori yönetimi için controller
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using dotnet_core_webservice.Data;
using dotnet_core_webservice.DTOs;
using dotnet_core_webservice.Models;

namespace dotnet_core_webservice.Controllers
{
    /// <summary>
    /// Kategori yönetimi API Controller
    /// Kategori CRUD işlemleri ve hierarchical kategori yapısı
    /// </summary>
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Constructor - Dependency Injection
        /// </summary>
        /// <param name="context">Entity Framework context</param>
        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tüm kategorileri listeler (hierarchical yapı ile)
        /// GET /api/category
        /// </summary>
        /// <returns>Kategori listesi</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryDto>), 200)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                Console.WriteLine("GetCategories endpoint called");
                
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .Include(c => c.Products)
                    .Include(c => c.ParentCategory)
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.Name)
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Slug = c.Slug,
                    ParentCategoryId = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                    SortOrder = c.SortOrder,
                    IsActive = c.IsActive,
                    ProductCount = c.Products!.Count(p => p.IsActive)
                })
                .ToListAsync();

            // Hierarchical yapıyı oluştur (ana kategoriler ve alt kategoriler)
            var mainCategories = categories.Where(c => c.ParentCategoryId == null).ToList();
            
            foreach (var mainCategory in mainCategories)
            {
                mainCategory.SubCategories = categories
                    .Where(c => c.ParentCategoryId == mainCategory.Id)
                    .ToList();
            }

            Console.WriteLine($"Returning {mainCategories.Count} main categories with subcategories");
            return Ok(mainCategories);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetCategories error: {ex.Message}");
            return BadRequest(new { message = "Failed to get categories", error = ex.Message });
        }
    }

        /// <summary>
        /// Sadece ana kategorileri listeler
        /// GET /api/category/main
        /// </summary>
        /// <returns>Ana kategori listesi</returns>
        [HttpGet("main")]
        [ProducesResponseType(typeof(List<CategoryDto>), 200)]
        public async Task<IActionResult> GetMainCategories()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive && c.ParentCategoryId == null)
                .Include(c => c.Products)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Slug = c.Slug,
                    ParentCategoryId = c.ParentCategoryId,
                    SortOrder = c.SortOrder,
                    IsActive = c.IsActive,
                    ProductCount = c.Products!.Count(p => p.IsActive)
                })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Belirli bir kategorinin alt kategorilerini listeler
        /// GET /api/category/{id}/subcategories
        /// </summary>
        /// <param name="id">Ana kategori ID'si</param>
        /// <returns>Alt kategori listesi</returns>
        [HttpGet("{id}/subcategories")]
        [ProducesResponseType(typeof(List<CategoryDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSubCategories(int id)
        {
            var parentCategory = await _context.Categories.FindAsync(id);
            if (parentCategory == null || !parentCategory.IsActive)
            {
                return NotFound(new { message = "Parent category not found" });
            }

            var subCategories = await _context.Categories
                .Where(c => c.IsActive && c.ParentCategoryId == id)
                .Include(c => c.Products)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Slug = c.Slug,
                    ParentCategoryId = c.ParentCategoryId,
                    ParentCategoryName = parentCategory.Name,
                    SortOrder = c.SortOrder,
                    IsActive = c.IsActive,
                    ProductCount = c.Products!.Count(p => p.IsActive)
                })
                .ToListAsync();

            return Ok(subCategories);
        }

        /// <summary>
        /// Belirli bir kategoriyi getirir
        /// GET /api/category/{id}
        /// </summary>
        /// <param name="id">Kategori ID'si</param>
        /// <returns>Kategori detayları</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoryDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories!.Where(sc => sc.IsActive))
                .Where(c => c.Id == id && c.IsActive)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Slug = c.Slug,
                    ParentCategoryId = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                    SortOrder = c.SortOrder,
                    IsActive = c.IsActive,
                    ProductCount = c.Products!.Count(p => p.IsActive),
                    SubCategories = c.SubCategories!.Select(sc => new CategoryDto
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        Description = sc.Description,
                        Slug = sc.Slug,
                        ParentCategoryId = sc.ParentCategoryId,
                        SortOrder = sc.SortOrder,
                        IsActive = sc.IsActive,
                        ProductCount = sc.Products!.Count(p => p.IsActive)
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            return Ok(category);
        }

        /// <summary>
        /// Yeni kategori oluşturur (Admin yetkisi gerekli)
        /// POST /api/category
        /// </summary>
        /// <param name="createCategoryDto">Kategori oluşturma bilgileri</param>
        /// <returns>Oluşturulan kategori</returns>
        [HttpPost]
        [Authorize] // JWT token gerekli
        [ProducesResponseType(typeof(CategoryDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Üst kategori kontrolü (varsa)
            if (createCategoryDto.ParentCategoryId.HasValue)
            {
                var parentExists = await _context.Categories
                    .AnyAsync(c => c.Id == createCategoryDto.ParentCategoryId.Value && c.IsActive);
                if (!parentExists)
                {
                    return BadRequest(new { message = "Parent category not found" });
                }
            }

            // Slug oluştur (basit versiyonu - production'da daha gelişmiş olmalı)
            var slug = createCategoryDto.Name
                .ToLowerInvariant()
                .Replace(' ', '-')
                .Replace('ç', 'c')
                .Replace('ğ', 'g')
                .Replace('ı', 'i')
                .Replace('ö', 'o')
                .Replace('ş', 's')
                .Replace('ü', 'u');

            // Slug benzersizlik kontrolü
            var originalSlug = slug;
            var counter = 1;
            while (await _context.Categories.AnyAsync(c => c.Slug == slug))
            {
                slug = $"{originalSlug}-{counter}";
                counter++;
            }

            var category = new Category
            {
                Name = createCategoryDto.Name,
                Description = createCategoryDto.Description,
                Slug = slug,
                ParentCategoryId = createCategoryDto.ParentCategoryId,
                SortOrder = createCategoryDto.SortOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var createdCategory = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Slug = category.Slug,
                ParentCategoryId = category.ParentCategoryId,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive,
                ProductCount = 0
            };

            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, createdCategory);
        }

        /// <summary>
        /// Kategori günceller
        /// PUT /api/category/{id}
        /// </summary>
        /// <param name="id">Güncellenecek kategori ID'si</param>
        /// <param name="updateCategoryDto">Güncelleme bilgileri</param>
        /// <returns>Güncellenmiş kategori</returns>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(CategoryDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDto updateCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null || !category.IsActive)
            {
                return NotFound(new { message = "Category not found" });
            }

            // Kendisini parent olarak seçmesini engelle
            if (updateCategoryDto.ParentCategoryId == id)
            {
                return BadRequest(new { message = "Category cannot be its own parent" });
            }

            // Circular reference kontrolü (basit versiyonu)
            if (updateCategoryDto.ParentCategoryId.HasValue)
            {
                var parentExists = await _context.Categories
                    .AnyAsync(c => c.Id == updateCategoryDto.ParentCategoryId.Value && c.IsActive);
                if (!parentExists)
                {
                    return BadRequest(new { message = "Parent category not found" });
                }
            }

            category.Name = updateCategoryDto.Name;
            category.Description = updateCategoryDto.Description;
            category.ParentCategoryId = updateCategoryDto.ParentCategoryId;
            category.SortOrder = updateCategoryDto.SortOrder;

            await _context.SaveChangesAsync();

            var updatedCategory = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Slug = category.Slug,
                ParentCategoryId = category.ParentCategoryId,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive,
                ProductCount = await _context.Products.CountAsync(p => p.CategoryId == id && p.IsActive)
            };

            return Ok(updatedCategory);
        }

        /// <summary>
        /// Kategoriyi siler (soft delete)
        /// DELETE /api/category/{id}
        /// </summary>
        /// <param name="id">Silinecek kategori ID'si</param>
        /// <returns>Silme işlemi sonucu</returns>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .Include(c => c.SubCategories)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            // Alt kategoriler varsa silmeyi engelle
            if (category.SubCategories?.Any(sc => sc.IsActive) == true)
            {
                return BadRequest(new { message = "Cannot delete category with active subcategories" });
            }

            // Aktif ürünler varsa silmeyi engelle
            if (category.Products?.Any(p => p.IsActive) == true)
            {
                return BadRequest(new { message = "Cannot delete category with active products" });
            }

            // Soft delete
            category.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully" });
        }

        /// <summary>
        /// Test endpoint - API çalışıp çalışmadığını kontrol eder
        /// GET /api/category/test
        /// </summary>
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { 
                message = "Category API is working!", 
                timestamp = DateTime.Now,
                database_connected = _context.Database.CanConnect()
            });
        }
    }
}
