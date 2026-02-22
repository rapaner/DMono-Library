using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Services;

namespace Library.ViewModels;

public partial class LoadingViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;

    public LoadingViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== LoadingVM: Starting database initialization ===");

            using var scope = _serviceProvider.CreateScope();
            var libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
            await libraryService.InitializeDatabaseAsync();

            System.Diagnostics.Debug.WriteLine("=== LoadingVM: Database initialized successfully ===");

            var shell = _serviceProvider.GetRequiredService<AppShell>();

            if (Application.Current?.Windows.Count > 0)
            {
                Application.Current.Windows[0].Page = shell;
            }

            System.Diagnostics.Debug.WriteLine("=== LoadingVM: Navigated to AppShell ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== LoadingVM: Error initializing database: {ex.Message} ===");
            System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");

            if (Application.Current?.Windows.Count > 0)
            {
                var page = Application.Current.Windows[0].Page;
                if (page != null)
                {
                    await page.DisplayAlertAsync(
                        "Ошибка инициализации",
                        $"Не удалось инициализировать базу данных:\n{ex.Message}",
                        "OK");
                }
            }

            Application.Current?.Quit();
        }
    }
}
