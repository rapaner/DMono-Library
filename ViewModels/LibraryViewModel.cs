using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Core.Models;
using Library.Services;
using Library.Views;
using System.Collections.ObjectModel;

namespace Library.ViewModels;

public partial class LibraryViewModel : ObservableObject
{
    private readonly IBookService _bookService;

    [ObservableProperty]
    private string _currentFilter = "All";

    [ObservableProperty]
    private int _selectedSortIndex;

    public ObservableCollection<BookItemViewModel> Books { get; } = new();

    public LibraryViewModel(IBookService bookService)
    {
        _bookService = bookService;
    }

    [RelayCommand]
    private async Task LoadBooksAsync()
    {
        Books.Clear();

        var allBooks = await _bookService.GetAllBooksAsync();

        List<Book> books = CurrentFilter switch
        {
            "Current" => allBooks.Where(b => b.Status == BookStatus.Reading).ToList(),
            "Planned" => allBooks.Where(b => b.Status == BookStatus.Planned).ToList(),
            "Finished" => allBooks.Where(b => b.Status == BookStatus.Finished).ToList(),
            _ => allBooks
        };

        string sort = SelectedSortIndex switch
        {
            1 => "Title",
            2 => "Author",
            _ => "Default"
        };

        books = sort switch
        {
            "Title" => books.OrderBy(b => b.Title).ToList(),
            "Author" => books.OrderBy(b => b.AuthorsText).ThenBy(b => b.Title).ToList(),
            _ => books
        };

        foreach (var book in books)
        {
            Books.Add(new BookItemViewModel(book));
        }
    }

    [RelayCommand]
    private async Task FilterAsync(string filter)
    {
        CurrentFilter = filter;
        await LoadBooksAsync();
    }

    partial void OnSelectedSortIndexChanged(int value)
    {
        _ = LoadBooksAsync();
    }

    [RelayCommand]
    private async Task SelectBookAsync(BookItemViewModel? bookItem)
    {
        if (bookItem == null) return;
        await Shell.Current.GoToAsync($"{nameof(BookDetailPage)}?bookId={bookItem.Id}");
    }

    [RelayCommand]
    private async Task AddBookAsync()
    {
        await Shell.Current.GoToAsync(nameof(AddEditBookPage));
    }
}
