using Library.Converters;
using Library.Core.Data;
using Library.Models;
using Library.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

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

            // Загрузка конфигурации из appsettings.json, appsettings.Development.json и переменных окружения
            var assembly = Assembly.GetExecutingAssembly();
            var configBuilder = new ConfigurationBuilder();

            // 1. Загрузка base appsettings.json
            using var stream = assembly.GetManifestResourceStream("Library.appsettings.json");
            if (stream != null)
            {
                configBuilder.AddJsonStream(stream);
                System.Diagnostics.Debug.WriteLine("=== Loaded appsettings.json ===");
            }

            // 2. Загрузка appsettings.Development.json для разработки (работает на всех платформах)
            using var devStream = assembly.GetManifestResourceStream("Library.appsettings.Development.json");
            if (devStream != null)
            {
                configBuilder.AddJsonStream(devStream);
                System.Diagnostics.Debug.WriteLine("=== Loaded appsettings.Development.json ===");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("=== appsettings.Development.json not found (optional) ===");
            }

            // 2.5. Загрузка appsettings.Release.json для Release сборки (высокий приоритет, переопределяет appsettings.json)
            using var releaseStream = assembly.GetManifestResourceStream("Library.appsettings.Release.json");
            if (releaseStream != null)
            {
                configBuilder.AddJsonStream(releaseStream);
                System.Diagnostics.Debug.WriteLine("=== Loaded appsettings.Release.json ===");
            }

            // 3. Переменные окружения (наивысший приоритет)
            configBuilder.AddEnvironmentVariables();

            var config = configBuilder.Build();
            builder.Configuration.AddConfiguration(config);

            System.Diagnostics.Debug.WriteLine("=== Configuration loaded ===");

            // Диагностика: выводим ключи Yandex OAuth
            System.Diagnostics.Debug.WriteLine("=== Yandex OAuth Configuration: ===");
            System.Diagnostics.Debug.WriteLine($"  YandexOAuthClientId = {config["YandexOAuthClientId"]}");

            // Создание и регистрация конфигурации приложения
            var appConfig = new AppConfiguration
            {
                AppDataDirectory = FileSystem.AppDataDirectory,
                DatabaseFileName = "library.db",
                DatabasePath = Path.Combine(FileSystem.AppDataDirectory, "library.db"),
                AppVersion = AppInfo.VersionString,
                AppName = AppInfo.Name
            };

            // Загрузка настроек из конфигурации
            var defaultReadingHours = builder.Configuration.GetSection("DefaultReadingHours");
            if (defaultReadingHours.Exists())
            {
                appConfig.DefaultStartHour = defaultReadingHours.GetValue<int>("StartHour", 6);
                appConfig.DefaultEndHour = defaultReadingHours.GetValue<int>("EndHour", 23);
                System.Diagnostics.Debug.WriteLine($"=== Loaded default reading hours from config: {appConfig.DefaultStartHour}-{appConfig.DefaultEndHour} ===");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("=== Using default reading hours: 6-23 ===");
            }

            // Загрузка настроек Yandex OAuth из конфигурации
            var yandexOAuthClientId = builder.Configuration.GetValue<string>("YandexOAuthClientId");
            if (string.IsNullOrEmpty(yandexOAuthClientId) is false)
            {
                appConfig.YandexOAuthClientId = yandexOAuthClientId ?? string.Empty;
                System.Diagnostics.Debug.WriteLine($"=== Loaded Yandex OAuth config: ClientId={appConfig.YandexOAuthClientId} ===");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("=== Yandex OAuth config not found ===");
            }

            builder.Services.AddSingleton(appConfig);

            System.Diagnostics.Debug.WriteLine($"=== App configuration created ===");
            System.Diagnostics.Debug.WriteLine($"=== Database path: {appConfig.DatabasePath} ===");
            System.Diagnostics.Debug.WriteLine($"=== App version: {appConfig.AppVersion} ===");

            // Регистрация базы данных
            var migrationsAssembly = typeof(LibraryDbContext).Assembly.GetName().Name;
            System.Diagnostics.Debug.WriteLine($"=== Migrations assembly: {migrationsAssembly} ===");

            // Общая строка подключения SQLite с повышенными таймаутами и без пула
            var csb = new SqliteConnectionStringBuilder
            {
                DataSource = appConfig.DatabasePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared,
                Pooling = false,
                DefaultTimeout = 60 // сек
            };
            var connectionString = csb.ToString();

            builder.Services.AddDbContext<LibraryDbContext>(options =>
            {
                options
                    .UseSqlite(connectionString, sqliteOptions => sqliteOptions.MigrationsAssembly(migrationsAssembly))
                    .EnableSensitiveDataLogging()
                    .LogTo(msg => System.Diagnostics.Debug.WriteLine(msg), LogLevel.Information);
            });

            // Фабрика контекстов для выполнения миграций в «чистом» контексте
            builder.Services.AddDbContextFactory<LibraryDbContext>(options =>
            {
                options
                    .UseSqlite(connectionString, sqliteOptions => sqliteOptions.MigrationsAssembly(migrationsAssembly))
                    .EnableSensitiveDataLogging()
                    .LogTo(msg => System.Diagnostics.Debug.WriteLine(msg), LogLevel.Information);
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
            builder.Services.AddTransient<Views.LoadingPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<Views.YandexDiskPage>();
            builder.Services.AddTransient<Views.ReadingSchedulePage>();
            builder.Services.AddTransient<Views.BookChoosePage>();

            // Сервисы выбора книги (keyed)
            builder.Services.AddKeyedSingleton<IBookChooseService, PrioritizedRandomBookChooseService>(Models.BookChooseServiceKey.PrioritizedRandomId);
            builder.Services.AddKeyedSingleton<IBookChooseService, RandomBookChooseService>(Models.BookChooseServiceKey.RandomId);
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