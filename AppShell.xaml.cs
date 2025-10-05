using Library.Views;

namespace Library;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Регистрация маршрутов для навигации
        Routing.RegisterRoute(nameof(YandexDiskPage), typeof(YandexDiskPage));
    }
}
