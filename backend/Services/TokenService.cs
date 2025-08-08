// JWT token işlemleri için gerekli using'ler
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using dotnet_core_webservice.Models;

namespace dotnet_core_webservice.Services
{
    /// <summary>
    /// JWT Token işlemlerini yöneten servis sınıfı
    /// Token üretme ve doğrulama işlemlerini gerçekleştirir
    /// </summary>
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor - Configuration dependency injection
        /// </summary>
        /// <param name="configuration">Uygulama yapılandırma servisi</param>
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Kullanıcı için JWT token üretir
        /// </summary>
        /// <param name="user">Token üretilecek kullanıcı</param>
        /// <returns>Base64 encoded JWT token string</returns>
        public string GenerateToken(User user)
        {
            // JWT token'a eklenecek claim'leri oluştur
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),                    // Kullanıcı adı
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())     // Kullanıcı ID'si
            };

            // JWT imzalama için symmetric key oluştur
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            
            // HMAC SHA256 algoritması ile imzalama credentials'ı oluştur
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // JWT token'ı oluştur
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],       // Token'ı kim yayınladı
                audience: _configuration["Jwt:Audience"],   // Token kimler için geçerli
                claims: claims,                             // Token içindeki kullanıcı bilgileri
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpirationMinutes"])), // Token süresi
                signingCredentials: creds                   // İmzalama bilgileri
            );

            // Token'ı string'e çevir ve döndür
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// JWT token'ı doğrular ve içindeki claim'leri döndürür
        /// </summary>
        /// <param name="token">Doğrulanacak JWT token</param>
        /// <returns>Token geçerliyse ClaimsPrincipal, değilse null</returns>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // JWT imzalama anahtarını al
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
            
            try
            {
                // Token doğrulama parametrelerini belirle
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,                    // İmzalama anahtarını doğrula
                    IssuerSigningKey = new SymmetricSecurityKey(key),   // İmzalama anahtarı
                    ValidateIssuer = true,                              // Issuer'ı doğrula
                    ValidateAudience = true,                            // Audience'ı doğrula
                    ValidIssuer = _configuration["Jwt:Issuer"],         // Beklenen issuer
                    ValidAudience = _configuration["Jwt:Audience"],     // Beklenen audience
                    ClockSkew = TimeSpan.Zero                          // Zaman farkı toleransı (0 = kesin)
                }, out SecurityToken validatedToken);

                return principal; // Doğrulama başarılı, claim'leri döndür
            }
            catch
            {
                // Token geçersiz veya hatalı
                return null;
            }
        }
    }
}