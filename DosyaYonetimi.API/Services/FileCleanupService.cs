using DosyaYonetimi.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DosyaYonetimi.API.Services
{
    public class FileCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebHostEnvironment _env;

        public FileCleanupService(IServiceProvider serviceProvider, IWebHostEnvironment env)
        {
            _serviceProvider = serviceProvider;
            _env = env;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            while (!stoppingToken.IsCancellationRequested)
            {
                
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    
                    var limitDate = DateTime.Now.AddDays(-14);

                    var expiredFiles = await context.Files
                        .Where(f => !f.IsActive && f.Updated < limitDate)
                        .ToListAsync();

                    if (expiredFiles.Any())
                    {
                        foreach (var file in expiredFiles)
                        {
                            try
                            {
                                
                                var filePath = Path.Combine(_env.ContentRootPath, "wwwroot/Files/UserUploads", file.SystemFileName);
                                if (System.IO.File.Exists(filePath))
                                {
                                    System.IO.File.Delete(filePath);
                                }

                                
                                context.Files.Remove(file);
                            }
                            catch (Exception ex)
                            {
                                
                                Console.WriteLine($"Otomatik temizleme hatası: {ex.Message}");
                            }
                        }

                        await context.SaveChangesAsync();
                        Console.WriteLine($"{expiredFiles.Count} adet eski dosya otomatik olarak temizlendi.");
                    }
                }
            }
        }
    }
}