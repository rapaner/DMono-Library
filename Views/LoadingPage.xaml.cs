using Library.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Views;

public partial class LoadingPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    public LoadingPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            System.Diagnostics.Debug.WriteLine("=== LoadingPage: Starting database initialization ===");

            // Инициализируем БД асинхронно
            using var scope = _serviceProvider.CreateScope();
            var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
            await libraryService.InitializeDatabaseAsync();

            System.Diagnostics.Debug.WriteLine("=== LoadingPage: Database initialized successfully ===");

            // Переходим к основному интерфейсу
            var shell = _serviceProvider.GetRequiredService<AppShell>();
            
            if (Application.Current?.Windows.Count > 0)
            {
                Application.Current.Windows[0].Page = shell;
            }

            System.Diagnostics.Debug.WriteLine("=== LoadingPage: Navigated to AppShell ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== LoadingPage: Error initializing database: {ex.Message} ===");
            System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");

            // Показываем ошибку пользователю
            await DisplayAlert(
                "Ошибка инициализации", 
                $"Не удалось инициализировать базу данных:\n{ex.Message}", 
                "OK");

            // Закрываем приложение
            Application.Current?.Quit();
        }
    }
}

