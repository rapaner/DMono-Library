using Library.Services;

namespace Library;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        System.Diagnostics.Debug.WriteLine("=== App constructor started ===");
        
        try
        {
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("=== App InitializeComponent completed ===");
            
            _serviceProvider = serviceProvider;
            
            // Инициализация базы данных при запуске приложения
            Task.Run(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("=== Starting database initialization ===");
                    using var scope = serviceProvider.CreateScope();
                    var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
                    await libraryService.EnsureDatabaseCreatedAsync();
                    System.Diagnostics.Debug.WriteLine("=== Database initialization completed ===");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"=== ERROR in database initialization: {ex.Message} ===");
                    System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
                }
            });
            
            System.Diagnostics.Debug.WriteLine("=== App constructor completed ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== ERROR in App constructor: {ex.Message} ===");
            System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
            throw;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== CreateWindow started ===");
            
            // Получаем AppShell через DI
            var shell = _serviceProvider.GetRequiredService<AppShell>();
            System.Diagnostics.Debug.WriteLine("=== AppShell obtained from DI ===");
            
            var window = new Window(shell);
            System.Diagnostics.Debug.WriteLine("=== Window created successfully ===");
            
            return window;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== ERROR in CreateWindow: {ex.Message} ===");
            System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
            throw;
        }
    }
}
