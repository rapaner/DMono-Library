using Library.Services;

namespace Library;

public partial class AppShell : Shell
{
    public AppShell(LibraryService libraryService)
    {
        InitializeComponent();
        
        // Инициализация базы данных при запуске приложения
        _ = Task.Run(async () =>
        {
            await libraryService.EnsureDatabaseCreatedAsync();
        });
    }
}
