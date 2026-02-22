using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Core.Models;
using Library.Services;

namespace Library.ViewModels;

public partial class UpdateProgressViewModel : ObservableObject, IQueryAttributable
{
    private readonly IBookService _bookService;
    private readonly IReadingProgressService _readingProgressService;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;
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

    public UpdateProgressViewModel(IBookService bookService, IReadingProgressService readingProgressService, INavigationService navigation, IDialogService dialog)
    {
        _bookService = bookService;
        _readingProgressService = readingProgressService;
        _navigation = navigation;
        _dialog = dialog;
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
        _book = await _bookService.GetBookByIdAsync(bookId);
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
            await _dialog.ShowAlertAsync("Ошибка", "Пожалуйста, введите номер страницы", "OK");
            return;
        }

        if (!int.TryParse(CurrentPageText, out int currentPage))
        {
            await _dialog.ShowAlertAsync("Ошибка", "Введите корректный номер страницы", "OK");
            return;
        }

        if (currentPage < 0 || currentPage > _book.TotalPages)
        {
            await _dialog.ShowAlertAsync("Ошибка", $"Номер страницы должен быть от 0 до {_book.TotalPages}", "OK");
            return;
        }

        try
        {
            await _readingProgressService.AddOrUpdateReadingProgressAsync(_book.Id, ReadingDate, currentPage);
            await _dialog.ShowAlertAsync("Успех", "Прогресс обновлен!", "OK");
            await _navigation.GoBackAsync();
        }
        catch (InvalidOperationException ex)
        {
            await _dialog.ShowAlertAsync("Ошибка", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await _dialog.ShowAlertAsync("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigation.GoBackAsync();
    }
}
