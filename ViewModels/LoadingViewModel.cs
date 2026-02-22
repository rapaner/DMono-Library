using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Services;

namespace Library.ViewModels;

public partial class LoadingViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDialogService _dialog;

    public LoadingViewModel(IServiceProvider serviceProvider, IDialogService dialog)
    {
        _serviceProvider = serviceProvider;
        _dialog = dialog;
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== LoadingVM: Starting database initialization ===");

            using var scope = _serviceProvider.CreateScope();
            var migrationService = scope.ServiceProvider.GetRequiredService<DatabaseMigrationService>();
            await migrationService.InitializeDatabaseAsync();

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

            try
            {
                await _dialog.ShowAlertAsync(
                    "Ошибка инициализации",
                    $"Не удалось инициализировать базу данных:\n{ex.Message}",
                    "OK");
            }
            catch
            {
                // Fallback if dialog service can't show (no page available yet)
            }

            Application.Current?.Quit();
        }
    }
}
