// ASP.NET Core hosting için gerekli using'ler
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using dotnet_core_webservice.Data;

/// <summary>
/// Uygulamanın ana entry point'i
/// Web API'yi başlatmak için gerekli yapılandırmaları içerir
/// </summary>
public class Program
{
    /// <summary>
    /// Uygulamanın başlangıç noktası
    /// Host builder'ı oluşturur ve uygulamayı çalıştırır
    /// </summary>
    /// <param name="args">Komut satırı argümanları</param>
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        // Database migration'ını uygula
        using (var scope = host.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            try
            {
                context.Database.EnsureCreated(); // Database'i oluştur
                Console.WriteLine("Database created/updated successfully");
                
                // Seed data ekle
                await ClothingDataSeeder.SeedClothingDataAsync(context);
                Console.WriteLine("Data seeding completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database operation failed: {ex.Message}");
            }
        }
        
        // Host builder'ı oluştur, build et ve uygulamayı çalıştır
        await host.RunAsync();
    }

    /// <summary>
    /// Web host yapılandırmasını oluşturur
    /// Startup sınıfını belirtir ve default konfigürasyonları kullanır
    /// </summary>
    /// <param name="args">Komut satırı argümanları</param>
    /// <returns>Yapılandırılmış IHostBuilder</returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args) // Default host yapılandırmasını kullan (logging, configuration vs.)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                // Startup sınıfını kullanarak web host'u yapılandır
                webBuilder.UseStartup<Startup>();
            });
}