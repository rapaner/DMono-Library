using Library.Models;
using Library.Services;

namespace Library.Views;

public partial class UpdateProgressPage : ContentPage
{
    private readonly LibraryService _libraryService;
    private readonly Book _book;
    private readonly Action _onProgressUpdated;

    public UpdateProgressPage(Book book, LibraryService libraryService, Action onProgressUpdated)
    {
        InitializeComponent();
        _libraryService = libraryService;
        _book = book;
        _onProgressUpdated = onProgressUpdated;
        
        LoadBookInfo();
    }

    private void LoadBookInfo()
    {
        BookTitleLabel.Text = _book.Title;
        BookAuthorLabel.Text = $"Автор: {_book.AuthorsText}";
        TotalPagesLabel.Text = $"Всего страниц: {_book.TotalPages}";
        CurrentProgressLabel.Text = $"Текущий прогресс: {_book.CurrentPage} / {_book.TotalPages} страниц ({_book.ProgressPercentage:F1}%)";
        
        // Устанавливаем текущую страницу как подсказку
        if (_book.CurrentPage > 0)
        {
            CurrentPageEntry.Text = _book.CurrentPage.ToString();
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Валидация ввода
        if (string.IsNullOrWhiteSpace(CurrentPageEntry.Text))
        {
            await DisplayAlert("Ошибка", "Пожалуйста, введите номер страницы", "OK");
            return;
        }

        if (!int.TryParse(CurrentPageEntry.Text, out int currentPage))
        {
            await DisplayAlert("Ошибка", "Введите корректный номер страницы", "OK");
            return;
        }

        if (currentPage < 0 || currentPage > _book.TotalPages)
        {
            await DisplayAlert("Ошибка", $"Номер страницы должен быть от 0 до {_book.TotalPages}", "OK");
            return;
        }

        try
        {
            // Обновляем прогресс через сервис
            var selectedDate = ReadingDatePicker.Date;
            await _libraryService.AddOrUpdateReadingProgressAsync(_book.Id, selectedDate, currentPage);
            
            await DisplayAlert("Успех", "Прогресс обновлен!", "OK");
            
            // Вызываем callback для обновления родительской страницы
            _onProgressUpdated?.Invoke();
            
            await Navigation.PopAsync();
        }
        catch (InvalidOperationException ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
