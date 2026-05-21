using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Services;
using Library.Views;
using System.Collections.ObjectModel;

namespace Library.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly IBookService _bookService;
    private readonly INavigationService _navigation;

    [ObservableProperty]
    private bool _hasCurrentBooks;

    [ObservableProperty]
    private bool _hasNoCurrentBooks = true;

    public ObservableCollection<BookItemViewModel> CurrentBooks { get; } = new();

    public MainPageViewModel(IBookService bookService, INavigationService navigation)
    {
        _bookService = bookService;
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task LoadCurrentBooksAsync()
    {
        var books = await _bookService.GetBooksByStatusAsync(true);

        CurrentBooks.Clear();
        foreach (var book in books)
        {
            CurrentBooks.Add(new BookItemViewModel(book));
        }

        HasCurrentBooks = books.Count > 0;
        HasNoCurrentBooks = books.Count == 0;
    }

    [RelayCommand]
    private async Task ViewBookAsync(BookItemViewModel? bookItem)
    {
        if (bookItem == null) return;
        await _navigation.GoToAsync($"{nameof(BookDetailPage)}?bookId={bookItem.Id}");
    }

    [RelayCommand]
    private async Task GoToLibraryAsync()
    {
        await _navigation.GoToAsync(nameof(LibraryPage));
    }

    [RelayCommand]
    private async Task GoToAddBookAsync()
    {
        await _navigation.GoToAsync(nameof(AddEditBookPage));
    }

    [RelayCommand]
    private async Task GoToStatisticsAsync()
    {
        await _navigation.GoToAsync(nameof(StatisticsPage));
    }

    [RelayCommand]
    private async Task GoToSettingsAsync()
    {
        await _navigation.GoToAsync(nameof(SettingsPage));
    }

    [RelayCommand]
    private async Task GoToBookChooseAsync()
    {
        await _navigation.GoToAsync(nameof(BookChoosePage));
    }

    [RelayCommand]
    private async Task GoToShelvesAsync()
    {
        await _navigation.GoToAsync(nameof(ShelvesPage));
    }

    [RelayCommand]
    private async Task GoToAuthorsAsync()
    {
        await _navigation.GoToAsync(nameof(AuthorsPage));
    }
}
