// Entity Framework Core için gerekli using'ler
using Microsoft.EntityFrameworkCore;
using dotnet_core_webservice.Models;

namespace dotnet_core_webservice.Data
{
    /// <summary>
    /// Entity Framework veritabanı context sınıfı
    /// E-commerce platformu için genişletilmiş veritabanı bağlantısını ve entity'leri yönetir
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// DbContext constructor
        /// Dependency injection ile DbContextOptions alır
        /// </summary>
        /// <param name="options">Veritabanı bağlantı seçenekleri</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Users tablosunu temsil eden DbSet
        /// Kullanıcı CRUD operasyonları için kullanılır
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Categories tablosunu temsil eden DbSet
        /// Ürün kategorileri için CRUD operasyonları
        /// </summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// Products tablosunu temsil eden DbSet
        /// Ürün yönetimi için CRUD operasyonları
        /// </summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>
        /// CartItems tablosunu temsil eden DbSet
        /// Alışveriş sepeti yönetimi için
        /// </summary>
        public DbSet<CartItem> CartItems { get; set; }

        /// <summary>
        /// Orders tablosunu temsil eden DbSet
        /// Sipariş yönetimi için CRUD operasyonları
        /// </summary>
        public DbSet<Order> Orders { get; set; }

        /// <summary>
        /// OrderItems tablosunu temsil eden DbSet
        /// Sipariş detayları için CRUD operasyonları
        /// </summary>
        public DbSet<OrderItem> OrderItems { get; set; }

        /// <summary>
        /// Model yapılandırması ve ilişkilerin tanımlanması
        /// Entity Framework mapping kuralları burada belirlenir
        /// </summary>
        /// <param name="modelBuilder">Model builder instance</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity konfigürasyonu
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique(); // Username benzersiz olmalı
                entity.HasIndex(u => u.Email).IsUnique();    // Email benzersiz olmalı (null değerler hariç)
            });

            // Category entity konfigürasyonu
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Slug).IsUnique(); // Slug benzersiz olmalı
                
                // Self-referencing relationship (Parent-Child kategoriler)
                entity.HasOne(c => c.ParentCategory)
                      .WithMany(c => c.SubCategories)
                      .HasForeignKey(c => c.ParentCategoryId)
                      .OnDelete(DeleteBehavior.Restrict); // Parent kategori silindiğinde child'ları silinmesin
            });

            // Product entity konfigürasyonu
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.SKU).IsUnique(); // SKU benzersiz olmalı
                
                // Product-Category relationship
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict); // Kategori silindiğinde ürünler silinmesin

                // Product-User relationship (CreatedBy)
                entity.HasOne(p => p.CreatedByUser)
                      .WithMany(u => u.CreatedProducts)
                      .HasForeignKey(p => p.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // CartItem entity konfigürasyonu
            modelBuilder.Entity<CartItem>(entity =>
            {
                // Composite unique index: Bir kullanıcının sepetinde aynı üründen sadece bir kayıt olsun
                entity.HasIndex(ci => new { ci.UserId, ci.ProductId }).IsUnique();

                // CartItem-User relationship
                entity.HasOne(ci => ci.User)
                      .WithMany(u => u.CartItems)
                      .HasForeignKey(ci => ci.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silinince sepeti de silinsin

                // CartItem-Product relationship
                entity.HasOne(ci => ci.Product)
                      .WithMany(p => p.CartItems)
                      .HasForeignKey(ci => ci.ProductId)
                      .OnDelete(DeleteBehavior.Cascade); // Ürün silinince sepetten de çıksın
            });

            // Order entity konfigürasyonu
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(o => o.OrderNumber).IsUnique(); // Sipariş numarası benzersiz

                // Order-User relationship
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinince siparişler korunsun
            });

            // OrderItem entity konfigürasyonu
            modelBuilder.Entity<OrderItem>(entity =>
            {
                // OrderItem-Order relationship
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade); // Sipariş silinince öğeleri de silinsin

                // OrderItem-Product relationship
                entity.HasOne(oi => oi.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict); // Ürün silinince sipariş geçmişi korunsun
            });

            // Decimal precision ayarları
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TaxAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TotalPrice)
                .HasPrecision(18, 2);
        }
    }
}