// Authentication ve JWT token işlemleri için gerekli using'ler
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Text;
using dotnet_core_webservice.Data;
using dotnet_core_webservice.Services;

/// <summary>
/// Uygulamanın startup yapılandırma sınıfı
/// Servisleri ve middleware'leri yapılandırır
/// </summary>
public class Startup
{
    /// <summary>
    /// Startup constructor - Configuration dependency injection
    /// </summary>
    /// <param name="configuration">Uygulama yapılandırma servisi</param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// Uygulama yapılandırma property'si
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Dependency Injection container'a servisleri ekler
    /// Bu method runtime tarafından otomatik olarak çağrılır
    /// </summary>
    /// <param name="services">Servis koleksiyonu</param>
    public void ConfigureServices(IServiceCollection services)
    {
        // CORS politikası ekle
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });

        // Entity Framework DbContext'i SQL Server ile yapılandır
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

        // MVC Controllers servisini ekle
        services.AddControllers();

        // Swagger/OpenAPI dokümantasyon servislerini ekle
        services.AddSwaggerGen(c =>
        {
            // Swagger dokümantasyon bilgileri
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "JWT Authentication API", Version = "v1" });
            
            // Swagger'a JWT authentication desteği ekle
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            // Swagger UI'da authorization requirement'ı tanımla
            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });

        // JWT Authentication yapılandırması
        services.AddAuthentication(options =>
        {
            // Default authentication scheme'i JWT Bearer olarak ayarla
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // JWT token doğrulama parametreleri
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,              // Token'ın issuer'ını doğrula
                ValidateAudience = true,            // Token'ın audience'ını doğrula
                ValidateLifetime = true,            // Token'ın süresini doğrula
                ValidateIssuerSigningKey = true,    // İmzalama anahtarını doğrula
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Audience"],
                // JWT secret key'i symmetric security key olarak ayarla
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")))
            };
        });

        // Custom servisleri dependency injection container'a ekle
        services.AddScoped<TokenService>();         // JWT token üretimi için
        services.AddScoped<IPasswordService, PasswordService>(); // Şifre hash/verify işlemleri için
    }

    /// <summary>
    /// HTTP request pipeline'ını yapılandırır
    /// Middleware'lerin sırasını belirler
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="env">Web host environment</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Development ortamında detaylı hata sayfası göster
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        
        // Swagger middleware'ini tüm ortamlarda aktif et (test için)
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "JWT Authentication API V1");
            c.RoutePrefix = string.Empty; // Swagger UI'yi root path'te ("/") göster
        });

        // Request routing middleware'i
        app.UseRouting();

        // CORS middleware'i - cross-origin requests için
        app.UseCors("AllowAll");

        // Authentication middleware'i - kimlik doğrulama
        app.UseAuthentication();
        
        // Authorization middleware'i - yetkilendirme
        app.UseAuthorization();

        // Endpoint mapping - controller'ları map et
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers(); // Tüm controller'ları otomatik olarak map et
        });
    }
}