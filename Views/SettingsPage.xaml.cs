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
            "Вы уверены, что хотите удалить все данные? Это действие нельзя отменить!\n\nРекомендуется создать резервную копию на Яндекс Диске перед удалением.", 
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

    private async void OnYandexDiskClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(YandexDiskPage));
    }
}
