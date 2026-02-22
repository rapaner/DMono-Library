using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Core.Models;
using Library.Services;

namespace Library.ViewModels;

public partial class UpdateProgressViewModel : ObservableObject, IQueryAttributable
{
    private readonly LibraryService _libraryService;
    private Book? _book;

    [ObservableProperty]
    private string _bookTitleText = string.Empty;

    [ObservableProperty]
    private string _bookAuthorText = string.Empty;

    [ObservableProperty]
    private string _totalPagesText = string.Empty;

    [ObservableProperty]
    private string _currentProgressText = string.Empty;

    [ObservableProperty]
    private string _currentPageText = string.Empty;

    [ObservableProperty]
    private DateTime _readingDate = DateTime.Today;

    public UpdateProgressViewModel(LibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("bookId", out var id))
        {
            int bookId;
            if (id is int intId) bookId = intId;
            else if (id is string strId && int.TryParse(strId, out var parsedId)) bookId = parsedId;
            else return;

            _ = LoadBookAsync(bookId);
        }
    }

    private async Task LoadBookAsync(int bookId)
    {
        _book = await _libraryService.GetBookByIdAsync(bookId);
        if (_book == null) return;

        BookTitleText = _book.Title;
        BookAuthorText = $"Автор: {_book.AuthorsText}";
        TotalPagesText = $"Всего страниц: {_book.TotalPages}";
        CurrentProgressText = $"Текущий прогресс: {_book.CurrentPage} / {_book.TotalPages} страниц ({_book.ProgressPercentage:F1}%)";

        if (_book.CurrentPage > 0)
            CurrentPageText = _book.CurrentPage.ToString();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_book == null) return;

        if (string.IsNullOrWhiteSpace(CurrentPageText))
        {
            await Shell.Current.DisplayAlertAsync("Ошибка", "Пожалуйста, введите номер страницы", "OK");
            return;
        }

        if (!int.TryParse(CurrentPageText, out int currentPage))
        {
            await Shell.Current.DisplayAlertAsync("Ошибка", "Введите корректный номер страницы", "OK");
            return;
        }

        if (currentPage < 0 || currentPage > _book.TotalPages)
        {
            await Shell.Current.DisplayAlertAsync("Ошибка", $"Номер страницы должен быть от 0 до {_book.TotalPages}", "OK");
            return;
        }

        try
        {
            await _libraryService.AddOrUpdateReadingProgressAsync(_book.Id, ReadingDate, currentPage);
            await Shell.Current.DisplayAlertAsync("Успех", "Прогресс обновлен!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (InvalidOperationException ex)
        {
            await Shell.Current.DisplayAlertAsync("Ошибка", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
