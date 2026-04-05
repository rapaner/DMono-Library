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
    private readonly IShelfService _shelfService;
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
    private ObservableCollection<Author> _selectedAuthors = new();

    [ObservableProperty]
    private ObservableCollection<Shelf> _allShelves = new();

    [ObservableProperty]
    private Shelf? _selectedShelf;

    public AddEditBookViewModel(IBookService bookService, IAuthorService authorService, IShelfService shelfService, INavigationService navigation, IDialogService dialog)
    {
        _bookService = bookService;
        _authorService = authorService;
        _shelfService = shelfService;
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

        var shelves = await _shelfService.GetAllShelvesAsync();
        AllShelves.Clear();
        foreach (var shelf in shelves)
            AllShelves.Add(shelf);

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
                BookStatus.FinishedLongAgo => 3,
                _ => 0
            };

            SelectedShelf = AllShelves.FirstOrDefault(s => s.Id == _book.ShelfId);
        }
        else
        {
            StatusPickerIndex = 0;
            SelectedShelf = AllShelves.FirstOrDefault(s => s.Id == 1);
        }
    }

    [RelayCommand]
    private async Task AddAuthorAsync()
    {
        var availableAuthors = _allAuthors
            .Where(a => !SelectedAuthors.Any(s => s.Id == a.Id))
            .ToList();

        if (availableAuthors.Count == 0)
        {
            await _dialog.ShowAlertAsync("Ошибка", "Выберите автора из списка", "OK");
            return;
        }

        var authorNames = availableAuthors.Select(a => a.Name).ToArray();
        var result = await _dialog.ShowActionSheetAsync("Выберите автора", "Отмена", null, authorNames);

        if (result == null || result == "Отмена")
            return;

        var author = availableAuthors.FirstOrDefault(a => a.Name == result);
        if (author != null)
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

            book.ShelfId = SelectedShelf?.Id ?? 1;

            var statusItems = new[] { "В планах", "Читаю сейчас", "Прочитано", "Прочитана давно" };
            var selectedStatus = StatusPickerIndex >= 0 && StatusPickerIndex < statusItems.Length
                ? statusItems[StatusPickerIndex]
                : "В планах";

            switch (selectedStatus)
            {
                case "Читаю сейчас":
                    book.IsCurrentlyReading = true;
                    book.DateFinished = null;
                    book.IsFinishedLongAgo = false;
                    break;
                case "Прочитано":
                    book.IsCurrentlyReading = false;
                    book.DateFinished ??= book.PagesReadHistory.Any()
                        ? book.PagesReadHistory.Max(p => p.Date)
                        : DateTime.Now;
                    book.IsFinishedLongAgo = false;
                    break;
                case "Прочитана давно":
                    book.IsCurrentlyReading = false;
                    book.DateFinished = null;
                    book.IsFinishedLongAgo = true;
                    break;
                default:
                    book.IsCurrentlyReading = false;
                    book.DateFinished = null;
                    book.IsFinishedLongAgo = false;
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
