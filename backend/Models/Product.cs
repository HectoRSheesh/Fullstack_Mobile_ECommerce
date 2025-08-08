// E-commerce ürün modeli
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_core_webservice.Models
{
    /// <summary>
    /// E-commerce platformundaki ürün varlık sınıfı
    /// Ürün bilgilerini ve stok durumunu yönetir
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Ürünün benzersiz kimlik numarası (Primary Key)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ürün adı - arama ve listeleme için kullanılır
        /// </summary>
        [Required]
        [StringLength(200)]
        public required string Name { get; set; }

        /// <summary>
        /// Ürün açıklaması - detay sayfasında gösterilir
        /// </summary>
        [StringLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// Ürün fiyatı - ondalıklı değer (18,2 precision)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Stok miktarı - satış kontrolü için kullanılır
        /// </summary>
        [Required]
        public int StockQuantity { get; set; }

        /// <summary>
        /// Ürün kategorisi ID'si (Foreign Key)
        /// </summary>
        [Required]
        public int CategoryId { get; set; }

        /// <summary>
        /// Kategori navigation property
        /// </summary>
        public virtual Category? Category { get; set; }

        /// <summary>
        /// Ürün resmi URL'i - opsiyonel
        /// </summary>
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Ürün SKU (Stock Keeping Unit) - benzersiz ürün kodu
        /// </summary>
        [StringLength(50)]
        public string? SKU { get; set; }

        /// <summary>
        /// Ürünün aktif durumu - satışa açık mı?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ürün oluşturma tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ürün güncelleme tarihi
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Ürünü oluşturan kullanıcı ID'si
        /// </summary>
        public int? CreatedByUserId { get; set; }

        /// <summary>
        /// Ürünü oluşturan kullanıcı navigation property
        /// </summary>
        public virtual User? CreatedByUser { get; set; }

        /// <summary>
        /// Bu ürünle ilgili sepet öğeleri
        /// </summary>
        public virtual ICollection<CartItem>? CartItems { get; set; }

        /// <summary>
        /// Bu ürünle ilgili sipariş öğeleri
        /// </summary>
        public virtual ICollection<OrderItem>? OrderItems { get; set; }
    }
}
