using Library.Views;

namespace Library;

public partial class AppShell : Shell
{
    public AppShell()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("=== AppShell constructor started ===");
            
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("=== AppShell InitializeComponent completed ===");
            
            // Регистрация маршрутов для навигации
            Routing.RegisterRoute(nameof(YandexDiskPage), typeof(YandexDiskPage));
            Routing.RegisterRoute(nameof(ReadingSchedulePage), typeof(ReadingSchedulePage));
            System.Diagnostics.Debug.WriteLine("=== AppShell routes registered ===");
            
            System.Diagnostics.Debug.WriteLine("=== AppShell constructor completed ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== ERROR in AppShell constructor: {ex.Message} ===");
            System.Diagnostics.Debug.WriteLine($"=== Stack trace: {ex.StackTrace} ===");
            throw;
        }
    }
}
