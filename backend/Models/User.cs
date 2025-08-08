// Data annotations için gerekli using
using System.ComponentModel.DataAnnotations;

namespace dotnet_core_webservice.Models
{
    /// <summary>
    /// Kullanıcı varlık sınıfı (Entity)
    /// Veritabanındaki Users tablosunu temsil eder
    /// E-commerce için genişletilmiş kullanıcı bilgileri
    /// </summary>
    public class User
    {
        /// <summary>
        /// Kullanıcının benzersiz kimlik numarası (Primary Key)
        /// Entity Framework tarafından otomatik olarak artırılan ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Kullanıcı adı - benzersiz olmalı
        /// Minimum 3, maksimum 50 karakter uzunluğunda
        /// </summary>
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public required string Username { get; set; }
        
        /// <summary>
        /// Kullanıcının hashlenmiş şifresi
        /// Güvenlik nedeniyle plain text olarak saklanmaz
        /// </summary>
        [Required]
        public required string PasswordHash { get; set; }

        /// <summary>
        /// E-posta adresi - benzersiz olmalı
        /// Sipariş bildirimleri ve hesap doğrulama için
        /// </summary>
        [StringLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// Kullanıcının tam adı
        /// Sipariş ve teslimat bilgileri için
        /// </summary>
        [StringLength(100)]
        public string? FullName { get; set; }

        /// <summary>
        /// Telefon numarası
        /// Sipariş bildirimleri ve kargo takibi için
        /// </summary>
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Varsayılan teslimat adresi
        /// Hızlı sipariş için kullanılır
        /// </summary>
        [StringLength(500)]
        public string? DefaultAddress { get; set; }

        /// <summary>
        /// Şehir bilgisi
        /// </summary>
        [StringLength(100)]
        public string? City { get; set; }

        /// <summary>
        /// Posta kodu
        /// </summary>
        [StringLength(20)]
        public string? PostalCode { get; set; }

        /// <summary>
        /// Kullanıcı rolü (Customer, Admin, Manager)
        /// </summary>
        [StringLength(20)]
        public string Role { get; set; } = "Customer";
        
        /// <summary>
        /// Kullanıcının hesap oluşturma tarihi
        /// Varsayılan olarak şu anki UTC tarihi atanır
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Kullanıcının son giriş yaptığı tarih
        /// İlk kayıtta null, sonraki girişlerde güncellenir
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
        
        /// <summary>
        /// Kullanıcının aktif durumu
        /// False ise kullanıcı hesabı devre dışı
        /// Varsayılan olarak true (aktif)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// E-posta doğrulama durumu
        /// </summary>
        public bool IsEmailVerified { get; set; } = false;

        /// <summary>
        /// Kullanıcının oluşturduğu ürünler (admin/satıcı için)
        /// </summary>
        public virtual ICollection<Product>? CreatedProducts { get; set; }

        /// <summary>
        /// Kullanıcının alışveriş sepeti öğeleri
        /// </summary>
        public virtual ICollection<CartItem>? CartItems { get; set; }

        /// <summary>
        /// Kullanıcının siparişleri
        /// </summary>
        public virtual ICollection<Order>? Orders { get; set; }
    }
}