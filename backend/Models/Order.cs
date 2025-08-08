// Sipariş modeli
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_core_webservice.Models
{
    /// <summary>
    /// Sipariş durumlarını tanımlayan enum
    /// </summary>
    public enum OrderStatus
    {
        Pending = 1,        // Beklemede
        Confirmed = 2,      // Onaylandı
        Processing = 3,     // İşleniyor
        Shipped = 4,        // Kargoya verildi
        Delivered = 5,      // Teslim edildi
        Cancelled = 6,      // İptal edildi
        Returned = 7        // İade edildi
    }

    /// <summary>
    /// Kullanıcı siparişlerini yöneten varlık sınıfı
    /// Sipariş bilgileri, durumu ve ödeme detaylarını içerir
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Siparişin benzersiz kimlik numarası (Primary Key)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Sipariş numarası - kullanıcıya gösterilen benzersiz kod
        /// Örnek: ORD-2025-0001
        /// </summary>
        [Required]
        [StringLength(50)]
        public required string OrderNumber { get; set; }

        /// <summary>
        /// Siparişi veren kullanıcı ID'si (Foreign Key)
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Kullanıcı navigation property
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// Sipariş tarihi
        /// </summary>
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Sipariş durumu
        /// </summary>
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>
        /// Toplam sipariş tutarı (ürünler + kargo)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Kargo ücreti
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; } = 0;

        /// <summary>
        /// Vergi tutarı
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0;

        /// <summary>
        /// Teslimat adresi
        /// </summary>
        [Required]
        [StringLength(500)]
        public required string ShippingAddress { get; set; }

        /// <summary>
        /// Teslimat şehri
        /// </summary>
        [Required]
        [StringLength(100)]
        public required string ShippingCity { get; set; }

        /// <summary>
        /// Teslimat posta kodu
        /// </summary>
        [StringLength(20)]
        public string? ShippingPostalCode { get; set; }

        /// <summary>
        /// Teslimat telefon numarası
        /// </summary>
        [StringLength(20)]
        public string? ShippingPhone { get; set; }

        /// <summary>
        /// Ödeme yöntemi
        /// </summary>
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Ödeme durumu
        /// </summary>
        public bool IsPaid { get; set; } = false;

        /// <summary>
        /// Ödeme tarihi
        /// </summary>
        public DateTime? PaidAt { get; set; }

        /// <summary>
        /// Sipariş notları - kullanıcı tarafından eklenen özel istekler
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Sipariş son güncelleme tarihi
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Siparişin öğeleri (ürünler)
        /// </summary>
        public virtual ICollection<OrderItem>? OrderItems { get; set; }

        /// <summary>
        /// Siparişteki toplam ürün sayısını döndürür
        /// </summary>
        [NotMapped]
        public int TotalItems => OrderItems?.Sum(oi => oi.Quantity) ?? 0;

        /// <summary>
        /// Siparişin iptal edilebilir olup olmadığını kontrol eder
        /// </summary>
        [NotMapped]
        public bool CanBeCancelled => Status == OrderStatus.Pending || Status == OrderStatus.Confirmed;
    }
}
