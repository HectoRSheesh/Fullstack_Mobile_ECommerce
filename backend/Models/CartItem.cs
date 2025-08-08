// Alışveriş sepeti öğesi modeli
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_core_webservice.Models
{
    /// <summary>
    /// Kullanıcının alışveriş sepetindeki ürün öğelerini yöneten varlık sınıfı
    /// Her kullanıcının sepetinde hangi ürünlerden kaç adet olduğunu tutar
    /// </summary>
    public class CartItem
    {
        /// <summary>
        /// Sepet öğesinin benzersiz kimlik numarası (Primary Key)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Sepet sahibi kullanıcı ID'si (Foreign Key)
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Kullanıcı navigation property
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// Sepetteki ürün ID'si (Foreign Key)
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Ürün navigation property
        /// </summary>
        public virtual Product? Product { get; set; }

        /// <summary>
        /// Sepetteki ürün adedi
        /// Minimum 1 olmalı
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        /// <summary>
        /// Ürünün sepete eklendiği tarih
        /// </summary>
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Sepet öğesinin son güncellenme tarihi
        /// Miktar değiştirildiğinde güncellenir
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Bu sepet öğesinin toplam fiyatını hesaplar
        /// Ürün fiyatı x miktar
        /// </summary>
        [NotMapped]
        public decimal TotalPrice => Product?.Price * Quantity ?? 0;

        /// <summary>
        /// Sepet öğesinin geçerli olup olmadığını kontrol eder
        /// Ürün aktif ve stokta mı?
        /// </summary>
        [NotMapped]
        public bool IsValid => Product?.IsActive == true && Product?.StockQuantity >= Quantity;
    }
}
