using Library.Converters;
using Library.Data;
using Library.Models;
using Library.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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

            // Создание и регистрация конфигурации приложения
            var appConfig = new AppConfiguration
            {
                AppDataDirectory = FileSystem.AppDataDirectory,
                DatabaseFileName = "library.db",
                DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "library.db"),
                AppVersion = AppInfo.VersionString,
                AppName = AppInfo.Name
            };
            
            builder.Services.AddSingleton(appConfig);
            
            System.Diagnostics.Debug.WriteLine($"=== App configuration created ===");
            System.Diagnostics.Debug.WriteLine($"=== Database path: {appConfig.DatabasePath} ===");
            System.Diagnostics.Debug.WriteLine($"=== App version: {appConfig.AppVersion} ===");

            // Регистрация базы данных
            var migrationsAssembly = typeof(MauiProgram).Assembly.GetName().Name;
            System.Diagnostics.Debug.WriteLine($"=== Migrations assembly: {migrationsAssembly} ===");
            
            builder.Services.AddDbContext<LibraryDbContext>(options =>
            {
                options.UseSqlite(
                    $"Data Source={appConfig.DatabasePath}",
                    sqliteOptions => sqliteOptions.MigrationsAssembly(migrationsAssembly));
                //options.ConfigureWarnings(builder =>
                //{
                //    builder.Ignore(RelationalEventId.PendingModelChangesWarning);
                //});
            });

            // Регистрация сервисов
            builder.Services.AddScoped<DatabaseMigrationService>();
            builder.Services.AddScoped<LibraryService>();
            builder.Services.AddSingleton<YandexDiskService>();
            builder.Services.AddSingleton<YandexOAuthService>();
            builder.Services.AddSingleton<SettingsService>();
            builder.Services.AddSingleton<PageByHourService>();

            // Регистрация конвертеров
            builder.Services.AddSingleton<PercentageConverter>();

            // Регистрация страниц и Shell
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<Views.YandexDiskPage>();
            builder.Services.AddTransient<Views.ReadingSchedulePage>();
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
