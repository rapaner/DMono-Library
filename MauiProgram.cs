using Library.Services;
using Library.Converters;
using Library.Data;
using Microsoft.EntityFrameworkCore;

namespace Library;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Регистрация базы данных
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "library.db");
        builder.Services.AddDbContext<LibraryDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Регистрация сервисов
        builder.Services.AddScoped<LibraryService>();
        builder.Services.AddSingleton<YandexDiskService>();
        builder.Services.AddSingleton<YandexOAuthService>();
        builder.Services.AddSingleton<SettingsService>();

        // Регистрация конвертеров
        builder.Services.AddSingleton<PercentageConverter>();

        // Регистрация страниц и Shell
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<Views.YandexDiskPage>();
        builder.Services.AddSingleton<AppShell>();

        return builder.Build();
    }
}
