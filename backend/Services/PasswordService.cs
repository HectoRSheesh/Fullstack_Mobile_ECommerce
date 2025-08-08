// BCrypt şifreleme kütüphanesi için using
using BCrypt.Net;

namespace dotnet_core_webservice.Services
{
    /// <summary>
    /// Şifre işlemleri için interface
    /// Dependency injection ve testability için kullanılır
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// Plain text şifreyi hash'ler
        /// </summary>
        /// <param name="password">Hashlenecek şifre</param>
        /// <returns>BCrypt ile hashlenmiş şifre</returns>
        string HashPassword(string password);
        
        /// <summary>
        /// Plain text şifreyi hash ile karşılaştırır
        /// </summary>
        /// <param name="password">Kontrol edilecek plain text şifre</param>
        /// <param name="hashedPassword">Veritabanındaki hashlenmiş şifre</param>
        /// <returns>Şifreler eşleşiyorsa true, değilse false</returns>
        bool VerifyPassword(string password, string hashedPassword);
    }

    /// <summary>
    /// BCrypt kullanarak şifre hash ve doğrulama işlemleri yapan servis
    /// IPasswordService interface'ini implement eder
    /// </summary>
    public class PasswordService : IPasswordService
    {
        /// <summary>
        /// Plain text şifreyi BCrypt algoritması ile hash'ler
        /// Her hash işleminde farklı salt üretir (rainbow table saldırılarına karşı güvenlik)
        /// </summary>
        /// <param name="password">Hashlenecek şifre</param>
        /// <returns>BCrypt ile hashlenmiş şifre</returns>
        /// <exception cref="ArgumentException">Şifre null veya boş ise fırlatılır</exception>
        public string HashPassword(string password)
        {
            // Null veya boş şifre kontrolü
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            // BCrypt ile şifreyi hashle (otomatik salt üretimi ile)
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        /// <summary>
        /// Plain text şifreyi hashlenmiş şifre ile karşılaştırır
        /// BCrypt'in dahili verify mekanizmasını kullanır
        /// </summary>
        /// <param name="password">Kontrol edilecek plain text şifre</param>
        /// <param name="hashedPassword">Veritabanındaki hashlenmiş şifre</param>
        /// <returns>Şifreler eşleşiyorsa true, değilse false</returns>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            // Null veya boş parametre kontrolü
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                // BCrypt verify işlemi - hash'i çözer ve karşılaştırır
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                // Hash bozuksa veya beklenmedik hata durumunda false döndür
                return false;
            }
        }
    }
}
