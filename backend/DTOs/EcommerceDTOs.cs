// E-commerce Data Transfer Objects
using System.ComponentModel.DataAnnotations;
using dotnet_core_webservice.Models;

namespace dotnet_core_webservice.DTOs
{
    // ===== PRODUCT DTOs =====

    /// <summary>
    /// Ürün oluşturma isteği için DTO
    /// </summary>
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public required string Name { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? ImageUrl { get; set; }

        [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
        public string? SKU { get; set; }
    }

    /// <summary>
    /// Ürün güncelleme isteği için DTO
    /// </summary>
    public class UpdateProductDto
    {
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
        public string? Name { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative")]
        public int? StockQuantity { get; set; }

        public int? CategoryId { get; set; }

        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        public string? ImageUrl { get; set; }

        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// Ürün listeleme/görüntüleme için DTO
    /// </summary>
    public class ProductDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? ImageUrl { get; set; }
        public string? SKU { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool InStock => StockQuantity > 0;
    }

    // ===== CATEGORY DTOs =====

    /// <summary>
    /// Kategori oluşturma isteği için DTO
    /// </summary>
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public required string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }

        public int SortOrder { get; set; } = 0;
    }

    /// <summary>
    /// Kategori görüntüleme için DTO
    /// </summary>
    public class CategoryDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Slug { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int ProductCount { get; set; }
        public List<CategoryDto>? SubCategories { get; set; }
    }

    // ===== CART DTOs =====

    /// <summary>
    /// Sepete ürün ekleme isteği için DTO
    /// </summary>
    public class AddToCartDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Sepet öğesi güncelleme için DTO
    /// </summary>
    public class UpdateCartItemDto
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Sepet öğesi görüntüleme için DTO
    /// </summary>
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => ProductPrice * Quantity;
        public bool IsAvailable { get; set; }
        public int AvailableStock { get; set; }
    }

    /// <summary>
    /// Sepet özeti için DTO
    /// </summary>
    public class CartSummaryDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public int TotalItems => Items.Sum(i => i.Quantity);
        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
        public decimal ShippingCost { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal GrandTotal => TotalAmount + ShippingCost + TaxAmount;
    }

    // ===== ORDER DTOs =====

    /// <summary>
    /// Sipariş oluşturma isteği için DTO
    /// </summary>
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Shipping address is required")]
        [StringLength(500, ErrorMessage = "Shipping address cannot exceed 500 characters")]
        public required string ShippingAddress { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public required string ShippingCity { get; set; }

        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string? ShippingPostalCode { get; set; }

        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        public string? ShippingPhone { get; set; }

        [StringLength(50, ErrorMessage = "Payment method cannot exceed 50 characters")]
        public string? PaymentMethod { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Sipariş görüntüleme için DTO
    /// </summary>
    public class OrderDto
    {
        public int Id { get; set; }
        public required string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public required string ShippingAddress { get; set; }
        public required string ShippingCity { get; set; }
        public string? ShippingPostalCode { get; set; }
        public string? ShippingPhone { get; set; }
        public string? PaymentMethod { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? Notes { get; set; }
        public int TotalItems { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public bool CanBeCancelled { get; set; }
    }

    /// <summary>
    /// Sipariş öğesi görüntüleme için DTO
    /// </summary>
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public string? ProductSKU { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Notes { get; set; }
    }

    // ===== PAGINATION DTO =====

    /// <summary>
    /// Sayfalama için genel DTO
    /// </summary>
    public class PagedResultDto<T>
    {
        public List<T> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }

    // ===== SEARCH/FILTER DTOs =====

    /// <summary>
    /// Ürün arama ve filtreleme için DTO
    /// </summary>
    public class ProductSearchDto
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? InStockOnly { get; set; } = true;
        public string? SortBy { get; set; } = "name"; // name, price, date
        public string? SortOrder { get; set; } = "asc"; // asc, desc
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Sipariş oluşturma için DTO
    /// </summary>
    public class CheckoutDto
    {
        public required string ShippingAddress { get; set; }
        public string? ShippingCity { get; set; }
        public string? PaymentMethod { get; set; }
        public List<CheckoutItemDto> OrderItems { get; set; } = new();
    }

    /// <summary>
    /// Sipariş öğesi oluşturma için DTO
    /// </summary>
    public class CheckoutItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
