// Sipariş öğesi modeli
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_core_webservice.Models
{
    /// <summary>
    /// Siparişin içindeki ürün öğelerini yöneten varlık sınıfı
    /// Her siparişteki ürün detaylarını (fiyat, miktar vs.) saklar
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// Sipariş öğesinin benzersiz kimlik numarası (Primary Key)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ait olduğu sipariş ID'si (Foreign Key)
        /// </summary>
        [Required]
        public int OrderId { get; set; }

        /// <summary>
        /// Sipariş navigation property
        /// </summary>
        public virtual Order? Order { get; set; }

        /// <summary>
        /// Sipariş edilen ürün ID'si (Foreign Key)
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Ürün navigation property
        /// </summary>
        public virtual Product? Product { get; set; }

        /// <summary>
        /// Sipariş anında ürünün adı
        /// Ürün adı sonradan değişirse sipariş geçmişi korunur
        /// </summary>
        [Required]
        [StringLength(200)]
        public required string ProductName { get; set; }

        /// <summary>
        /// Sipariş anında ürünün birim fiyatı
        /// Fiyat değişse bile sipariş tutarı sabit kalır
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Sipariş edilen ürün miktarı
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        /// <summary>
        /// Bu öğenin toplam tutarı (birim fiyat x miktar)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Sipariş öğesi oluşturma tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ürün SKU'su sipariş anında (değişebilir ürün bilgileri için)
        /// </summary>
        [StringLength(50)]
        public string? ProductSKU { get; set; }

        /// <summary>
        /// Bu öğe için özel notlar (renk, beden vs.)
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
