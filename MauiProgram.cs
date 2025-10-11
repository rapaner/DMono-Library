using Library.Services;
using Library.Converters;
using Library.Data;
using Microsoft.EntityFrameworkCore;

namespace Library;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== CreateMauiApp started ===");
            
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    // Шрифты временно закомментированы, так как файлы отсутствуют
                    // fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    // fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            System.Diagnostics.Debug.WriteLine("=== MAUI builder configured ===");

            // Регистрация базы данных
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "library.db");
            System.Diagnostics.Debug.WriteLine($"=== Database path: {dbPath} ===");
            
            builder.Services.AddDbContext<LibraryDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Регистрация пути к базе данных как singleton
            builder.Services.AddSingleton(dbPath);

            // Регистрация сервисов
            builder.Services.AddScoped<LibraryService>(sp =>
            {
                var context = sp.GetRequiredService<LibraryDbContext>();
                var path = sp.GetRequiredService<string>();
                return new LibraryService(context, path);
            });
            builder.Services.AddSingleton<YandexDiskService>();
            builder.Services.AddSingleton<YandexOAuthService>();
            builder.Services.AddSingleton<SettingsService>();

            // Регистрация конвертеров
            builder.Services.AddSingleton<PercentageConverter>();

            // Регистрация страниц и Shell
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<Views.YandexDiskPage>();
            builder.Services.AddSingleton<AppShell>();

            System.Diagnostics.Debug.WriteLine("=== Services registered ===");

            var app = builder.Build();
            
            System.Diagnostics.Debug.WriteLine("=== CreateMauiApp completed successfully ===");
            
            return app;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== ERROR in CreateMauiApp: {ex.Message} ===");
            System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
            throw;
        }
    }
}
