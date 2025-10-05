using Library.Services;

namespace Library;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
        
        // Инициализация базы данных при запуске приложения
        Task.Run(async () =>
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
                await libraryService.EnsureDatabaseCreatedAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка инициализации БД: {ex.Message}");
            }
        });
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Получаем AppShell через DI
        var shell = _serviceProvider.GetRequiredService<AppShell>();
        return new Window(shell);
    }
}
