// E-commerce kategori modeli
using System.ComponentModel.DataAnnotations;

namespace dotnet_core_webservice.Models
{
    /// <summary>
    /// Ürün kategorilerini yöneten varlık sınıfı
    /// Hierarchical kategori yapısını destekler
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Kategorinin benzersiz kimlik numarası (Primary Key)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Kategori adı - menülerde ve filtrelerde gösterilir
        /// </summary>
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        /// <summary>
        /// Kategori açıklaması - opsiyonel
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// URL-friendly kategori slug'ı (SEO için)
        /// Örnek: "elektronik-urunler"
        /// </summary>
        [StringLength(100)]
        public string? Slug { get; set; }

        /// <summary>
        /// Üst kategori ID'si - alt kategori yapısı için
        /// Null ise ana kategoridir
        /// </summary>
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// Üst kategori navigation property
        /// </summary>
        public virtual Category? ParentCategory { get; set; }

        /// <summary>
        /// Alt kategoriler collection'ı
        /// </summary>
        public virtual ICollection<Category>? SubCategories { get; set; }

        /// <summary>
        /// Bu kategorideki ürünler
        /// </summary>
        public virtual ICollection<Product>? Products { get; set; }

        /// <summary>
        /// Kategori sıralama numarası - menülerde sıralama için
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Kategori aktif durumu
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Kategori oluşturma tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Kategorideki toplam ürün sayısını döndürür
        /// </summary>
        public int ProductCount => Products?.Count(p => p.IsActive) ?? 0;
    }
}
