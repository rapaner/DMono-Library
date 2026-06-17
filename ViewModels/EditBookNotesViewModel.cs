using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Core.Models;
using Library.Services;

namespace Library.ViewModels;

public partial class EditBookNotesViewModel : ObservableObject, IQueryAttributable
{
    private readonly IBookService _bookService;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;
    private Book? _book;

    [ObservableProperty]
    private string _notes = string.Empty;

    public EditBookNotesViewModel(IBookService bookService, INavigationService navigation, IDialogService dialog)
    {
        _bookService = bookService;
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
        if (_book != null)
            Notes = _book.Notes;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_book == null) return;

        try
        {
            _book.Notes = Notes.Trim();
            await _bookService.UpdateBookAsync(_book);
            await _dialog.ShowAlertAsync("Успех", "Книга обновлена!", "OK");
            await _navigation.GoBackAsync();
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
