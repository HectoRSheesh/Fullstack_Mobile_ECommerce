// Data validation attributes için gerekli using
using System.ComponentModel.DataAnnotations;

namespace dotnet_core_webservice.DTOs
{
    /// <summary>
    /// Kullanıcı giriş isteği için Data Transfer Object
    /// API endpoint'ine gelen login verilerini alır
    /// </summary>
    public class UserLoginDto
    {
        /// <summary>
        /// E-posta adresi - giriş için gerekli
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public required string Email { get; set; }

        /// <summary>
        /// Kullanıcı şifresi - giriş için gerekli
        /// Minimum 6 karakter olmalı
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public required string Password { get; set; }
    }

    /// <summary>
    /// Kullanıcı kayıt isteği için Data Transfer Object
    /// Yeni hesap oluşturma verilerini alır
    /// </summary>
    public class UserRegisterDto
    {
        /// <summary>
        /// Kullanıcının tam adı
        /// </summary>
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        public required string FullName { get; set; }

        /// <summary>
        /// Kullanıcı adı - benzersiz olmalı
        /// </summary>
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public required string Username { get; set; }

        /// <summary>
        /// E-posta adresi - benzersiz olmalı
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100)]
        public required string Email { get; set; }

        /// <summary>
        /// Kullanıcı şifresi
        /// Güvenlik için minimum 6 karakter gerekli
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public required string Password { get; set; }

        /// <summary>
        /// Şifre onayı - Password ile aynı olmalı
        /// </summary>
        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
        public required string ConfirmPassword { get; set; }

        /// <summary>
        /// Telefon numarası (opsiyonel)
        /// </summary>
        [StringLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// Adres bilgisi (opsiyonel)
        /// </summary>
        [StringLength(500)]
        public string? Address { get; set; }
    }

    /// <summary>
    /// Başarılı giriş/kayıt sonrası dönen yanıt
    /// JWT token ve kullanıcı bilgilerini içerir
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// JWT authentication token'ı
        /// Korumalı endpoint'lere erişim için kullanılır
        /// </summary>
        public required string Token { get; set; }
        
        /// <summary>
        /// Kullanıcı bilgileri (hassas veriler hariç)
        /// </summary>
        public required UserDto User { get; set; }
        
        /// <summary>
        /// İşlem sonucu mesajı
        /// Kullanıcıya gösterilmek üzere
        /// </summary>
        public required string Message { get; set; }
    }

    /// <summary>
    /// Kullanıcı bilgileri için DTO
    /// Hassas veriler (şifre hash'i gibi) içermez
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// Kullanıcının benzersiz ID'si
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Kullanıcının tam adı
        /// </summary>
        public required string FullName { get; set; }
        
        /// <summary>
        /// E-posta adresi
        /// </summary>
        public required string Email { get; set; }
        
        /// <summary>
        /// Telefon numarası
        /// </summary>
        public string? Phone { get; set; }
        
        /// <summary>
        /// Adres bilgisi
        /// </summary>
        public string? Address { get; set; }
        
        /// <summary>
        /// Hesap oluşturma tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Son giriş tarihi (varsa)
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
        
        /// <summary>
        /// Hesap aktif durumu
        /// </summary>
        public bool IsActive { get; set; }
    }
}
