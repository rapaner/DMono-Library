using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Core.Models;
using Library.Services;
using System.Collections.ObjectModel;

namespace Library.ViewModels;

public partial class AddEditBookViewModel : ObservableObject, IQueryAttributable
{
    private readonly IBookService _bookService;
    private readonly IAuthorService _authorService;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;
    private Book? _book;
    private bool _isEditMode;
    private List<Author> _allAuthors = new();

    [ObservableProperty]
    private string _pageTitle = "Добавить книгу";

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _totalPagesText = string.Empty;

    [ObservableProperty]
    private string _seriesTitle = string.Empty;

    [ObservableProperty]
    private string _seriesNumberText = string.Empty;

    [ObservableProperty]
    private int _statusPickerIndex;

    [ObservableProperty]
    private int _authorPickerIndex = -1;

    [ObservableProperty]
    private ObservableCollection<string> _authorNames = new();

    [ObservableProperty]
    private ObservableCollection<Author> _selectedAuthors = new();

    public AddEditBookViewModel(IBookService bookService, IAuthorService authorService, INavigationService navigation, IDialogService dialog)
    {
        _bookService = bookService;
        _authorService = authorService;
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

            _isEditMode = true;
            PageTitle = "Редактировать книгу";
            _ = LoadBookForEditAsync(bookId);
        }
        else
        {
            _ = LoadDataAsync();
        }
    }

    private async Task LoadBookForEditAsync(int bookId)
    {
        _book = await _bookService.GetBookByIdAsync(bookId);
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        _allAuthors = await _authorService.GetAllAuthorsAsync();
        AuthorNames = new ObservableCollection<string>(_allAuthors.Select(a => a.Name));

        if (_isEditMode && _book != null)
        {
            Title = _book.Title;
            TotalPagesText = _book.TotalPages.ToString();
            SeriesTitle = _book.SeriesTitle ?? string.Empty;
            SeriesNumberText = _book.SeriesNumber?.ToString() ?? string.Empty;

            SelectedAuthors.Clear();
            foreach (var author in _book.Authors)
                SelectedAuthors.Add(author);

            StatusPickerIndex = _book.Status switch
            {
                BookStatus.Reading => 1,
                BookStatus.Finished => 2,
                _ => 0
            };
        }
        else
        {
            StatusPickerIndex = 0;
        }
    }

    [RelayCommand]
    private async Task AddAuthorAsync()
    {
        if (AuthorPickerIndex < 0)
        {
            await _dialog.ShowAlertAsync("Ошибка", "Выберите автора из списка", "OK");
            return;
        }

        var selectedAuthorName = AuthorNames.ElementAtOrDefault(AuthorPickerIndex);
        var author = _allAuthors.FirstOrDefault(a => a.Name == selectedAuthorName);

        if (author != null && !SelectedAuthors.Any(a => a.Id == author.Id))
        {
            SelectedAuthors.Add(author);
        }
    }

    [RelayCommand]
    private async Task NewAuthorAsync()
    {
        var result = await _dialog.ShowPromptAsync("Новый автор", "Введите имя автора:", "Добавить", "Отмена");

        if (!string.IsNullOrWhiteSpace(result))
        {
            var existingAuthor = _allAuthors.FirstOrDefault(a => a.Name == result);
            if (existingAuthor != null)
            {
                await _dialog.ShowAlertAsync("Информация", "Такой автор уже существует", "OK");
                return;
            }

            var newAuthor = await _authorService.AddAuthorAsync(new Author { Name = result });
            _allAuthors.Add(newAuthor);
            SelectedAuthors.Add(newAuthor);

            AuthorNames = new ObservableCollection<string>(_allAuthors.Select(a => a.Name));

            await _dialog.ShowAlertAsync("Успех", "Автор добавлен!", "OK");
        }
    }

    [RelayCommand]
    private void RemoveAuthor(Author? author)
    {
        if (author != null)
        {
            SelectedAuthors.Remove(author);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            await _dialog.ShowAlertAsync("Ошибка", "Пожалуйста, введите название книги", "OK");
            return;
        }

        if (SelectedAuthors.Count == 0)
        {
            await _dialog.ShowAlertAsync("Ошибка", "Пожалуйста, выберите хотя бы одного автора", "OK");
            return;
        }

        if (!int.TryParse(TotalPagesText, out int totalPages) || totalPages <= 0)
        {
            await _dialog.ShowAlertAsync("Ошибка", "Пожалуйста, введите корректное количество страниц", "OK");
            return;
        }

        try
        {
            var book = _book ?? new Book();

            book.Title = Title.Trim();
            book.TotalPages = totalPages;
            book.SeriesTitle = string.IsNullOrWhiteSpace(SeriesTitle) ? null : SeriesTitle.Trim();
            book.SeriesNumber = int.TryParse(SeriesNumberText, out int seriesNumber) ? seriesNumber : null;

            book.Authors.Clear();
            foreach (var author in SelectedAuthors)
                book.Authors.Add(author);

            var statusItems = new[] { "В планах", "Читаю сейчас", "Прочитано" };
            var selectedStatus = StatusPickerIndex >= 0 && StatusPickerIndex < statusItems.Length
                ? statusItems[StatusPickerIndex]
                : "В планах";

            switch (selectedStatus)
            {
                case "Читаю сейчас":
                    book.IsCurrentlyReading = true;
                    book.DateFinished = null;
                    break;
                case "Прочитано":
                    book.IsCurrentlyReading = false;
                    book.DateFinished = DateTime.Now;
                    break;
                default:
                    book.IsCurrentlyReading = false;
                    book.DateFinished = null;
                    break;
            }

            if (book.IsCurrentlyReading)
                await _bookService.SetCurrentBookAsync(book);

            if (_isEditMode)
                await _bookService.UpdateBookAsync(book);
            else
                await _bookService.AddBookAsync(book);

            await _dialog.ShowAlertAsync("Успех",
                _isEditMode ? "Книга обновлена!" : "Книга добавлена!",
                "OK");

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
