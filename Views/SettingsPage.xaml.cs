using Library.Services;

namespace Library.Views;

public partial class SettingsPage : ContentPage
{
    private readonly LibraryService _libraryService;

    public SettingsPage(LibraryService libraryService)
    {
        InitializeComponent();
        _libraryService = libraryService;
    }

    private async void OnClearDataClicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Подтверждение", 
            "Вы уверены, что хотите удалить все данные? Это действие нельзя отменить!", 
            "Да, удалить", "Отмена");
            
        if (result)
        {
        try
        {
            var allBooks = await _libraryService.GetAllBooksAsync();
            foreach (var book in allBooks)
            {
                await _libraryService.DeleteBookAsync(book);
            }
            
            await DisplayAlert("Успех", "Все данные удалены!", "OK");
        }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
            }
        }
    }

    private async void OnExportDataClicked(object sender, EventArgs e)
    {
        try
        {
            var allBooks = await _libraryService.GetAllBooksAsync();
            var json = System.Text.Json.JsonSerializer.Serialize(allBooks, new System.Text.Json.JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            var fileName = $"library_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            
            await File.WriteAllTextAsync(filePath, json);
            
            await DisplayAlert("Успех", $"Данные экспортированы в файл: {fileName}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка при экспорте: {ex.Message}", "OK");
        }
    }

    private async void OnImportDataClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Информация", "Функция импорта данных будет добавлена в следующих версиях", "OK");
    }

    private async void OnContactSupportClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Поддержка", "Для связи с поддержкой отправьте email на адрес: support@libraryapp.com", "OK");
    }
}
