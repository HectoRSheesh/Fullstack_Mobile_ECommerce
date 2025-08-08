// ASP.NET Core MVC ve Authorization için gerekli using'ler
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using dotnet_core_webservice.Models;
using dotnet_core_webservice.Services;
using dotnet_core_webservice.Data;
using dotnet_core_webservice.DTOs;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace dotnet_core_webservice.Controllers
{
    /// <summary>
    /// Authentication işlemleri için API Controller
    /// Kullanıcı kayıt, giriş ve yetkilendirme endpoint'lerini içerir
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly IPasswordService _passwordService;
        private readonly TokenService _tokenService;

        /// <summary>
        /// Constructor - Dependency Injection
        /// </summary>
        /// <param name="dbContext">Entity Framework veritabanı context'i</param>
        /// <param name="passwordService">Şifre hash/verify işlemleri için servis</param>
        /// <param name="tokenService">JWT token üretimi için servis</param>
        public AuthController(
            AppDbContext dbContext, 
            IPasswordService passwordService, 
            TokenService tokenService)
        {
            _dbContext = dbContext;
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Yeni kullanıcı kayıt endpoint'i
        /// POST /api/auth/register
        /// </summary>
        /// <param name="registerDto">Kullanıcı kayıt bilgileri</param>
        /// <returns>Başarılı kayıt sonrası JWT token ve kullanıcı bilgileri</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(typeof(object), 400)]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto registerDto)
        {
            try
            {
                // Null check for registerDto
                if (registerDto == null)
                {
                    return BadRequest(new { message = "Registration data is required" });
                }

                // Debug logging
                Console.WriteLine($"Register attempt for: {registerDto.Email}");
                Console.WriteLine($"Model State Valid: {ModelState.IsValid}");
                
                // Model validation kontrolü
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value?.Errors.Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>() })
                        .ToArray();
                    
                    Console.WriteLine($"Validation errors: {string.Join(", ", errors.Select(e => $"{e.Field}: {string.Join(", ", e.Errors)}"))}");
                    return BadRequest(new { message = "Validation failed", errors = errors });
                }

                // Null check for required fields
                if (string.IsNullOrEmpty(registerDto.Email) || string.IsNullOrEmpty(registerDto.Password))
                {
                    return BadRequest(new { message = "Email and password are required" });
                }

                // E-posta adresinin daha önce kullanılıp kullanılmadığını kontrol et
                var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
                if (existingUser != null)
                {
                    Console.WriteLine($"Email already exists: {registerDto.Email}");
                    return BadRequest(new { message = "Email address already exists" });
                }

                Console.WriteLine("Creating new user...");

                // Yeni kullanıcı oluştur
                var user = new User
                {
                    Username = registerDto.Email, // Email'i username olarak kullan
                    Email = registerDto.Email,
                    FullName = registerDto.FullName,
                    PhoneNumber = registerDto.Phone,
                    DefaultAddress = registerDto.Address,
                    PasswordHash = _passwordService.HashPassword(registerDto.Password), // Şifreyi hashle
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Kullanıcıyı veritabanına ekle ve kaydet
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();

                Console.WriteLine($"User created with ID: {user.Id}");

                // Yeni kullanıcı için JWT token üret
                var token = _tokenService.GenerateToken(user);

                // Response DTO'sunu oluştur
                var userDto = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Address = user.DefaultAddress,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive
                };
                
                var response = new LoginResponseDto
                {
                    Token = token,
                    User = userDto,
                    Message = "Registration successful"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Register error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new { message = "Registration failed", error = ex.Message });
            }
        }

        /// <summary>
        /// Kullanıcı giriş endpoint'i
        /// POST /api/auth/login
        /// </summary>
        /// <param name="loginDto">Kullanıcı giriş bilgileri</param>
        /// <returns>Başarılı giriş sonrası JWT token ve kullanıcı bilgileri</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), 200)]
        [ProducesResponseType(typeof(object), 401)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                // Debug logging
                Console.WriteLine($"Login attempt for: {loginDto?.Email}");
                Console.WriteLine($"Model State Valid: {ModelState.IsValid}");
                
                // Null check for loginDto
                if (loginDto == null)
                {
                    Console.WriteLine("LoginDto is null");
                    return BadRequest(new { message = "Login data is required" });
                }

                // Model validation kontrolü
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value?.Errors.Select(e => e.ErrorMessage) ?? Enumerable.Empty<string>() })
                        .ToArray();
                    
                    Console.WriteLine($"Validation errors: {string.Join(", ", errors.Select(e => $"{e.Field}: {string.Join(", ", e.Errors)}"))}");
                    return BadRequest(new { message = "Validation failed", errors = errors });
                }

                Console.WriteLine($"Searching for user with email: {loginDto.Email}");

                // Kullanıcıyı email ile bul ve aktif olup olmadığını kontrol et
                var user = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

                if (user == null)
                {
                    Console.WriteLine($"User not found or inactive for email: {loginDto.Email}");
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                Console.WriteLine($"User found: {user.Email}, verifying password...");

                // Şifre kontrolü
                if (!_passwordService.VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    Console.WriteLine("Password verification failed");
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                Console.WriteLine("Password verified successfully, updating last login...");

                // Son giriş zamanını güncelle
                user.LastLoginAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                // JWT token üret
                var token = _tokenService.GenerateToken(user);

                Console.WriteLine($"Token generated successfully for user: {user.Email}");

                // Response DTO'sunu oluştur
                var response = new LoginResponseDto
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName ?? "",
                        Email = user.Email ?? "",
                        Phone = user.PhoneNumber,
                        Address = user.DefaultAddress,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt,
                        IsActive = user.IsActive
                    },
                    Message = "Login successful"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest(new { message = "Login failed", error = ex.Message });
            }
        }

        /// <summary>
        /// Test endpoint - API çalışıp çalışmadığını kontrol eder
        /// GET /api/auth/test
        /// </summary>
        [HttpGet("test")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> Test()
        {
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync();
                var userCount = canConnect ? await _dbContext.Users.CountAsync() : -1;
                
                return Ok(new { 
                    Message = "Auth API is working!", 
                    Timestamp = DateTime.Now,
                    DatabaseConnected = canConnect,
                    UserCount = userCount,
                    ConnectionString = _dbContext.Database.GetConnectionString()
                });
            }
            catch (Exception ex)
            {
                return Ok(new { 
                    Message = "Auth API is working but database error!", 
                    Timestamp = DateTime.Now,
                    DatabaseConnected = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Korumalı test endpoint'i
        /// GET /api/auth/protected
        /// JWT token gerektirir ([Authorize] attribute ile korunur)
        /// </summary>
        /// <returns>Kimlik doğrulanmış kullanıcı bilgileri</returns>
        [HttpGet("protected")]
        [Authorize] // JWT token zorunlu
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        public IActionResult Protected()
        {
            // JWT token'dan kullanıcı adını al
            var username = User.Identity?.Name ?? "Unknown";
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            return Ok(new
            {
                Message = "This is a protected endpoint",
                Username = username,
                UserId = userId,
                Claims = User.Claims.Select(c => new { c.Type, c.Value }),
                Timestamp = DateTime.Now
            });
        }

        /// <summary>
        /// Veritabanı bağlantı test endpoint'i
        /// GET /api/auth/db-test
        /// Veritabanı bağlantısını test eder ve kullanıcı sayısını döndürür
        /// </summary>
        /// <returns>Veritabanı bağlantı durumu ve istatistikleri</returns>
        [HttpGet("db-test")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> DatabaseTest()
        {
            try
            {
                // Veritabanı bağlantısını test et
                var canConnect = await _dbContext.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return Ok(new
                    {
                        Message = "Database connection failed",
                        Connected = false,
                        Timestamp = DateTime.Now
                    });
                }

                // Kullanıcı sayısını al
                var userCount = await _dbContext.Users.CountAsync();
                
                // Son eklenen kullanıcıları al (varsa)
                var recentUsers = await _dbContext.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(3)
                    .Select(u => new { u.Id, u.Username, u.CreatedAt, u.IsActive })
                    .ToListAsync();

                return Ok(new
                {
                    Message = "Database connection successful",
                    Connected = true,
                    UserCount = userCount,
                    RecentUsers = recentUsers,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Message = "Database test failed",
                    Connected = false,
                    Error = ex.Message,
                    Timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// Kullanıcının profil bilgilerini getirir
        /// GET /auth/profile
        /// </summary>
        /// <returns>Kullanıcı profil bilgileri</returns>
        [HttpGet("profile")]
        [Authorize] // JWT token gerekli
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                Console.WriteLine("GetProfile endpoint called");
                
                // JWT token'dan kullanıcı ID'sini al
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    Console.WriteLine("GetProfile: Invalid user token");
                    return Unauthorized(new { message = "Invalid user token" });
                }

                Console.WriteLine($"GetProfile: Getting profile for user ID: {userId}");

                // Kullanıcı bilgilerini getir
                var user = await _dbContext.Users
                    .Where(u => u.Id == userId && u.IsActive)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Email = u.Email ?? "",
                        FullName = u.FullName ?? "",
                        Phone = u.PhoneNumber,
                        Address = u.DefaultAddress,
                        CreatedAt = u.CreatedAt,
                        LastLoginAt = u.LastLoginAt,
                        IsActive = u.IsActive
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    Console.WriteLine($"GetProfile: User not found for ID: {userId}");
                    return NotFound(new { message = "User not found" });
                }

                Console.WriteLine($"GetProfile: Returning profile for user: {user.Email}");
                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetProfile error: {ex.Message}");
                return BadRequest(new { message = "Failed to get profile", error = ex.Message });
            }
        }
    }
}
