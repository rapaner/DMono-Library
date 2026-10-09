using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Core.Models;
using Library.Models;
using Library.Services;
using Library.Views;
using System.Collections.ObjectModel;

namespace Library.ViewModels;

public partial class BookChooseViewModel : ObservableObject
{
    #region Поля

    private readonly IServiceProvider _serviceProvider;
    private readonly SettingsService _settingsService;
    private readonly IBookService _bookService;
    private readonly IShelfService _shelfService;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;

    private List<Book> _allBooks = [];
    private List<Book> _filteredBooks = [];
    private int _booksAmount;
    private int _lastChosenBookNumber;
    private int _currentBookChooseServiceOption;
    private int _lastBookId;
    private int _chosenBookId;
    private bool _suppressFilterUpdates;

    #endregion Поля

    #region Конструкторы

    /// <summary>
    /// Создаёт модель представления страницы выбора книги.
    /// </summary>
    public BookChooseViewModel(
        IServiceProvider serviceProvider,
        SettingsService settingsService,
        IBookService bookService,
        IShelfService shelfService,
        INavigationService navigation,
        IDialogService dialog)
    {
        _serviceProvider = serviceProvider;
        _settingsService = settingsService;
        _bookService = bookService;
        _shelfService = shelfService;
        _navigation = navigation;
        _dialog = dialog;

        var (booksAmount, lastChosen, currentOption, lastBookId) = _settingsService.GetBookChooseSettings();
        _booksAmount = booksAmount;
        _lastChosenBookNumber = lastChosen;
        _currentBookChooseServiceOption = currentOption;
        _lastBookId = lastBookId;
        _chosenBookId = lastBookId;

        foreach (var key in BookChooseServiceKey.GetAll())
            ServiceKeys.Add(key);

        foreach (var status in BookChooseStatusFilterOption.GetAll())
            StatusFilters.Add(status);

        ChosenBookNumberText = _lastChosenBookNumber > 0 ? _lastChosenBookNumber.ToString() : string.Empty;
        BooksAmountText = _booksAmount.ToString();
        SelectedServiceIndex = _currentBookChooseServiceOption >= 0 ? _currentBookChooseServiceOption : -1;
        SelectedStatusIndex = 0;
    }

    #endregion Конструкторы

    #region Свойства

    /// <summary>
    /// Текст количества книг для расчёта.
    /// </summary>
    [ObservableProperty]
    private string _booksAmountText = string.Empty;

    /// <summary>
    /// Номер выбранной книги.
    /// </summary>
    [ObservableProperty]
    private string _chosenBookNumberText = string.Empty;

    /// <summary>
    /// Название выбранной книги.
    /// </summary>
    [ObservableProperty]
    private string _chosenBookTitle = string.Empty;

    /// <summary>
    /// Авторы выбранной книги.
    /// </summary>
    [ObservableProperty]
    private string _chosenBookAuthors = string.Empty;

    /// <summary>
    /// Показывать карточку выбранной книги.
    /// </summary>
    [ObservableProperty]
    private bool _isChosenBookVisible;

    /// <summary>
    /// Индекс выбранного варианта расчёта.
    /// </summary>
    [ObservableProperty]
    private int _selectedServiceIndex = -1;

    /// <summary>
    /// Индекс выбранной полки в фильтре.
    /// </summary>
    [ObservableProperty]
    private int _selectedShelfIndex;

    /// <summary>
    /// Индекс выбранного статуса в фильтре.
    /// </summary>
    [ObservableProperty]
    private int _selectedStatusIndex;

    /// <summary>
    /// Варианты расчёта.
    /// </summary>
    public ObservableCollection<BookChooseServiceKey> ServiceKeys { get; } = new();

    /// <summary>
    /// Варианты фильтра по полке.
    /// </summary>
    public ObservableCollection<BookChooseShelfOption> ShelfOptions { get; } = new();

    /// <summary>
    /// Варианты фильтра по статусу.
    /// </summary>
    public ObservableCollection<BookChooseStatusFilterOption> StatusFilters { get; } = new();

    #endregion Свойства

    #region Методы

    partial void OnBooksAmountTextChanged(string value)
    {
        if (int.TryParse(value, out int parsed))
            _booksAmount = parsed;
    }

    partial void OnSelectedServiceIndexChanged(int value)
    {
        _currentBookChooseServiceOption = value;
    }

    partial void OnSelectedShelfIndexChanged(int value)
    {
        if (_suppressFilterUpdates)
            return;

        ApplyFilters();
        UpdateBooksAmountFromFilters();
    }

    partial void OnSelectedStatusIndexChanged(int value)
    {
        if (_suppressFilterUpdates)
            return;

        ApplyFilters();
        UpdateBooksAmountFromFilters();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        _suppressFilterUpdates = true;

        _allBooks = await _bookService.GetAllBooksAsync();
        var shelves = await _shelfService.GetAllShelvesAsync();

        ShelfOptions.Clear();
        ShelfOptions.Add(new BookChooseShelfOption(null, "Все книги"));
        foreach (var shelf in shelves.OrderBy(s => s.Name))
            ShelfOptions.Add(new BookChooseShelfOption(shelf.Id, shelf.Name));

        if (SelectedShelfIndex < 0 || SelectedShelfIndex >= ShelfOptions.Count)
            SelectedShelfIndex = 0;

        if (SelectedStatusIndex < 0 || SelectedStatusIndex >= StatusFilters.Count)
            SelectedStatusIndex = 0;

        _suppressFilterUpdates = false;

        ApplyFilters();
        UpdateBooksAmountFromFilters();

        if (_lastBookId > 0 && _lastChosenBookNumber > 0)
            await RestoreLastChosenBookAsync();
    }

    [RelayCommand]
    private async Task CalculateAsync()
    {
        ApplyFilters();

        if (_currentBookChooseServiceOption < 0)
            return;

        if (_filteredBooks.Count == 0)
            return;

        if (_booksAmount <= 0)
            return;

        if (_booksAmount > _filteredBooks.Count)
        {
            await _dialog.ShowAlertAsync(
                "Ошибка",
                $"Количество не может быть больше {_filteredBooks.Count} (книг по выбранным фильтрам).",
                "OK");
            return;
        }

        var chooseService = _serviceProvider.GetRequiredKeyedService<IBookChooseService>(_currentBookChooseServiceOption);
        int result = await chooseService.ChooseBook(_booksAmount);

        var book = _filteredBooks[result - 1];
        SetChosenBook(book, result);

        _lastChosenBookNumber = result;
        _lastBookId = book.Id;

        _settingsService.SaveBookChooseSettings(
            _booksAmount,
            _lastChosenBookNumber,
            _currentBookChooseServiceOption,
            _lastBookId);
    }

    [RelayCommand]
    private async Task OpenBookAsync()
    {
        if (_chosenBookId <= 0)
            return;

        await _navigation.GoToAsync($"{nameof(BookDetailPage)}?bookId={_chosenBookId}");
    }

    private void ApplyFilters()
    {
        if (ShelfOptions.Count == 0 || StatusFilters.Count == 0)
        {
            _filteredBooks = [];
            return;
        }

        var shelfOption = ShelfOptions[SelectedShelfIndex];
        var statusOption = StatusFilters[SelectedStatusIndex];

        IEnumerable<Book> query = _allBooks;

        if (shelfOption.ShelfId is int shelfId)
            query = query.Where(b => b.ShelfId == shelfId);

        if (statusOption.Status is BookStatus status)
            query = query.Where(b => b.Status == status);

        _filteredBooks = query.OrderBy(b => b.DateAdded).ToList();
    }

    private void UpdateBooksAmountFromFilters()
    {
        _booksAmount = _filteredBooks.Count;
        BooksAmountText = _booksAmount.ToString();
    }

    private void SetChosenBook(Book book, int number)
    {
        _chosenBookId = book.Id;
        ChosenBookNumberText = number.ToString();
        ChosenBookTitle = book.Title;
        ChosenBookAuthors = book.AuthorsText;
        IsChosenBookVisible = true;
    }

    private async Task RestoreLastChosenBookAsync()
    {
        var book = await _bookService.GetBookByIdAsync(_lastBookId);
        if (book == null)
        {
            _lastBookId = 0;
            _lastChosenBookNumber = 0;
            _chosenBookId = 0;
            IsChosenBookVisible = false;
            ChosenBookNumberText = string.Empty;
            ChosenBookTitle = string.Empty;
            ChosenBookAuthors = string.Empty;
            _settingsService.SaveBookChooseSettings(_booksAmount, 0, _currentBookChooseServiceOption, 0);
            return;
        }

        SetChosenBook(book, _lastChosenBookNumber);
    }

    #endregion Методы
}
