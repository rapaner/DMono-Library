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

            Routing.RegisterRoute(nameof(LibraryPage), typeof(LibraryPage));
            Routing.RegisterRoute(nameof(BookDetailPage), typeof(BookDetailPage));
            Routing.RegisterRoute(nameof(AddEditBookPage), typeof(AddEditBookPage));
            Routing.RegisterRoute(nameof(StatisticsPage), typeof(StatisticsPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(BookChoosePage), typeof(BookChoosePage));
            Routing.RegisterRoute(nameof(UpdateProgressPage), typeof(UpdateProgressPage));
            Routing.RegisterRoute(nameof(ReadingHistoryEditPage), typeof(ReadingHistoryEditPage));
            Routing.RegisterRoute(nameof(ReadingSchedulePage), typeof(ReadingSchedulePage));
            Routing.RegisterRoute(nameof(AlternativePageCalculationPage), typeof(AlternativePageCalculationPage));
            Routing.RegisterRoute(nameof(YandexDiskPage), typeof(YandexDiskPage));

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