using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Core.Models;
using Library.Services;
using Library.Views;
using System.Collections.ObjectModel;

namespace Library.ViewModels;

public partial class AddEditShelfViewModel : ObservableObject, IQueryAttributable
{
    private readonly IShelfService _shelfService;
    private readonly IBookService _bookService;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;
    private Shelf? _shelf;
    private bool _isEditMode;
    private List<Book> _allShelfBooks = new();

    [ObservableProperty]
    private string _pageTitle = "Новая полка";

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private bool _isDeleteVisible;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _hasBooks;

    [ObservableProperty]
    private bool _isEmpty = true;

    public ObservableCollection<BookItemViewModel> Books { get; } = new();

    public AddEditShelfViewModel(IShelfService shelfService, IBookService bookService, INavigationService navigation, IDialogService dialog)
    {
        _shelfService = shelfService;
        _bookService = bookService;
        _navigation = navigation;
        _dialog = dialog;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("shelfId", out var id))
        {
            int shelfId;
            if (id is int intId) shelfId = intId;
            else if (id is string strId && int.TryParse(strId, out var parsedId)) shelfId = parsedId;
            else return;

            _isEditMode = true;
            PageTitle = "Редактировать полку";
            IsDeleteVisible = true;
            _ = LoadShelfAsync(shelfId);
        }
    }

    private async Task LoadShelfAsync(int shelfId)
    {
        _shelf = await _shelfService.GetShelfByIdAsync(shelfId);
        if (_shelf != null)
        {
            Name = _shelf.Name;
            await LoadBooksAsync();
        }
    }

    [RelayCommand]
    public async Task LoadBooksAsync()
    {
        Books.Clear();

        if (_shelf == null)
        {
            _allShelfBooks.Clear();
            HasBooks = false;
            IsEmpty = true;
            return;
        }

        _allShelfBooks = await _bookService.GetBooksByShelfIdAsync(_shelf.Id);

        HasBooks = _allShelfBooks.Count > 0;
        IsEmpty = _allShelfBooks.Count == 0;

        var filtered = _allShelfBooks.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(b => b.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var book in filtered)
        {
            Books.Add(new BookItemViewModel(book));
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadBooksAsync();
    }

    [RelayCommand]
    private async Task SelectBookAsync(BookItemViewModel? bookItem)
    {
        if (bookItem == null) return;
        await _navigation.GoToAsync($"{nameof(BookDetailPage)}?bookId={bookItem.Id}");
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await _dialog.ShowAlertAsync("Ошибка", "Пожалуйста, введите название полки", "OK");
            return;
        }

        try
        {
            var shelf = _shelf ?? new Shelf();
            shelf.Name = Name.Trim();

            if (_isEditMode)
                await _shelfService.UpdateShelfAsync(shelf);
            else
                await _shelfService.AddShelfAsync(shelf);

            await _dialog.ShowAlertAsync("Успех",
                _isEditMode ? "Полка обновлена!" : "Полка добавлена!",
                "OK");

            await _navigation.GoBackAsync();
        }
        catch (Exception ex)
        {
            await _dialog.ShowAlertAsync("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (_shelf == null) return;

        var canDelete = await _shelfService.CanDeleteShelfAsync(_shelf.Id);
        if (!canDelete)
        {
            await _dialog.ShowAlertAsync("Ошибка", "Невозможно удалить полку, на которой есть книги. Сначала переместите все книги на другую полку.", "OK");
            return;
        }

        bool result = await _dialog.ShowConfirmAsync("Подтверждение",
            $"Вы уверены, что хотите удалить полку \"{_shelf.Name}\"?",
            "Да", "Нет");

        if (result)
        {
            await _shelfService.DeleteShelfAsync(_shelf);
            await _dialog.ShowAlertAsync("Успех", "Полка удалена!", "OK");
            await _navigation.GoBackAsync();
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigation.GoBackAsync();
    }
}
