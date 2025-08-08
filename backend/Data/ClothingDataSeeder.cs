using Microsoft.EntityFrameworkCore;
using dotnet_core_webservice.Data;
using dotnet_core_webservice.Models;

namespace dotnet_core_webservice.Data
{
    /// <summary>
    /// Clothing kategorisi ve ürünlerini veritabanına ekleyen script
    /// </summary>
    public static class ClothingDataSeeder
    {
        /// <summary>
        /// Clothing kategorisi ve ürünlerini ekler
        /// </summary>
        /// <param name="context">Database context</param>
        public static async Task SeedClothingDataAsync(AppDbContext context)
        {
            try
            {
                // Clothing ana kategorisini kontrol et ve ekle
                var clothingCategory = await context.Categories
                    .FirstOrDefaultAsync(c => c.Name == "Clothing");

                if (clothingCategory == null)
                {
                    clothingCategory = new Category
                    {
                        Name = "Clothing",
                        Description = "Fashion and clothing items",
                        Slug = "clothing",
                        IsActive = true,
                        SortOrder = 10,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Categories.Add(clothingCategory);
                    await context.SaveChangesAsync();
                }

                // Alt kategoriler ekle
                await AddSubCategoriesAsync(context, clothingCategory.Id);
                
                // Ürünler ekle
                await AddClothingProductsAsync(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClothingDataSeeder Error: {ex.Message}");
                throw;
            }
        }

        private static async Task AddSubCategoriesAsync(AppDbContext context, int parentId)
        {
            var subCategories = new[]
            {
                new { Name = "Men's Clothing", Description = "Men's fashion and clothing", Slug = "mens-clothing", Sort = 1 },
                new { Name = "Women's Clothing", Description = "Women's fashion and clothing", Slug = "womens-clothing", Sort = 2 },
                new { Name = "Kids' Clothing", Description = "Children's fashion and clothing", Slug = "kids-clothing", Sort = 3 }
            };

            foreach (var subCat in subCategories)
            {
                var existing = await context.Categories
                    .FirstOrDefaultAsync(c => c.Name == subCat.Name && c.ParentCategoryId == parentId);

                if (existing == null)
                {
                    var category = new Category
                    {
                        Name = subCat.Name,
                        Description = subCat.Description,
                        Slug = subCat.Slug,
                        ParentCategoryId = parentId,
                        IsActive = true,
                        SortOrder = subCat.Sort,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Categories.Add(category);
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task AddClothingProductsAsync(AppDbContext context)
        {
            // Kategorileri al
            var mensCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Men's Clothing");
            var womensCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Women's Clothing");
            var kidsCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Kids' Clothing");

            if (mensCategory == null || womensCategory == null || kidsCategory == null)
            {
                Console.WriteLine("Categories not found, skipping product seeding");
                return;
            }

            var products = new[]
            {
                // Men's products
                new Product
                {
                    Name = "Men's Classic T-Shirt",
                    Description = "100% cotton classic fit t-shirt. Comfortable and durable for everyday wear.",
                    Price = 29.99m,
                    StockQuantity = 150,
                    CategoryId = mensCategory.Id,
                    SKU = "MTS001",
                    ImageUrl = "/images/mens-tshirt-classic.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Men's Denim Jeans",
                    Description = "Classic blue denim jeans with regular fit. Perfect for casual and semi-formal occasions.",
                    Price = 79.99m,
                    StockQuantity = 80,
                    CategoryId = mensCategory.Id,
                    SKU = "MJN001",
                    ImageUrl = "/images/mens-jeans-denim.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // Women's products
                new Product
                {
                    Name = "Women's Floral Dress",
                    Description = "Beautiful floral pattern dress made from lightweight fabric. Perfect for summer occasions.",
                    Price = 89.99m,
                    StockQuantity = 75,
                    CategoryId = womensCategory.Id,
                    SKU = "WFD001",
                    ImageUrl = "/images/womens-floral-dress.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Women's Skinny Jeans",
                    Description = "High-waisted skinny jeans with stretch fabric for comfort and style.",
                    Price = 79.99m,
                    StockQuantity = 120,
                    CategoryId = womensCategory.Id,
                    SKU = "WSJ001",
                    ImageUrl = "/images/womens-skinny-jeans.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                // Kids' products
                new Product
                {
                    Name = "Kids' Cartoon T-Shirt",
                    Description = "Fun cartoon-themed t-shirt made from soft cotton. Perfect for active kids.",
                    Price = 19.99m,
                    StockQuantity = 200,
                    CategoryId = kidsCategory.Id,
                    SKU = "KTS001",
                    ImageUrl = "/images/kids-cartoon-tshirt.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Kids' Denim Overalls",
                    Description = "Classic denim overalls for kids. Durable and comfortable for play time.",
                    Price = 49.99m,
                    StockQuantity = 80,
                    CategoryId = kidsCategory.Id,
                    SKU = "KDO001",
                    ImageUrl = "/images/kids-denim-overalls.jpg",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var product in products)
            {
                var existing = await context.Products.FirstOrDefaultAsync(p => p.SKU == product.SKU);
                if (existing == null)
                {
                    context.Products.Add(product);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
