using Library.Models;
using Library.Services;
using System.Collections.ObjectModel;

namespace Library.Views;

public partial class LibraryPage : ContentPage
{
    private readonly LibraryService _libraryService;
    private ObservableCollection<BookViewModel> _books;
    private string _currentFilter = "All";

    public LibraryPage(LibraryService libraryService)
    {
        InitializeComponent();
        _libraryService = libraryService;
        _books = new ObservableCollection<BookViewModel>();
        BooksCollectionView.ItemsSource = _books;
        
        _ = LoadBooks();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadBooks();
    }

    private async Task LoadBooks()
    {
        _books.Clear();
        
        var allBooks = await _libraryService.GetAllBooksAsync();
        
        List<Book> books = _currentFilter switch
        {
            "Current" => allBooks.Where(b => b.Status == BookStatus.Reading).ToList(),
            "Planned" => allBooks.Where(b => b.Status == BookStatus.Planned).ToList(),
            "Finished" => allBooks.Where(b => b.Status == BookStatus.Finished).ToList(),
            _ => allBooks
        };

        foreach (var book in books)
        {
            _books.Add(new BookViewModel(book));
        }
    }

    private async void OnFilterChanged(object sender, EventArgs e)
    {
        var button = sender as Button;
        
        // Сбросить стили всех кнопок
        AllBooksButton.BackgroundColor = GetThemeColor("CardBackgroundColor", Color.FromArgb("#F0F0F0"));
        AllBooksButton.TextColor = GetThemeColor("SecondaryTextColor", Color.FromArgb("#333333"));
        CurrentBooksButton.BackgroundColor = GetThemeColor("CardBackgroundColor", Color.FromArgb("#F0F0F0"));
        CurrentBooksButton.TextColor = GetThemeColor("SecondaryTextColor", Color.FromArgb("#333333"));
        PlannedBooksButton.BackgroundColor = GetThemeColor("CardBackgroundColor", Color.FromArgb("#F0F0F0"));
        PlannedBooksButton.TextColor = GetThemeColor("SecondaryTextColor", Color.FromArgb("#333333"));
        FinishedBooksButton.BackgroundColor = GetThemeColor("CardBackgroundColor", Color.FromArgb("#F0F0F0"));
        FinishedBooksButton.TextColor = GetThemeColor("SecondaryTextColor", Color.FromArgb("#333333"));

        // Установить стиль активной кнопки
        if (button != null)
        {
            button.BackgroundColor = GetThemeColor("PrimaryColor", Color.FromArgb("#512BD4"));
            button.TextColor = GetThemeColor("SecondaryTextColor", Colors.White);
        }

        // Обновить фильтр
        _currentFilter = button?.Text switch
        {
            "Читаю сейчас" => "Current",
            "В планах" => "Planned",
            "Прочитано" => "Finished",
            _ => "All"
        };

        await LoadBooks();
    }

    private async void OnBookSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is BookViewModel selectedBook)
        {
            var book = await _libraryService.GetBookByIdAsync(selectedBook.Id);
            if (book != null)
            {
                await Navigation.PushAsync(new BookDetailPage(book, _libraryService));
            }
        }
        
        BooksCollectionView.SelectedItem = null;
    }

    private async void OnAddBookClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddEditBookPage(_libraryService));
    }

    private Color GetThemeColor(string resourceKey, Color defaultColor)
    {
        if (Application.Current?.Resources.TryGetValue(resourceKey, out var color) == true && color is Color themeColor)
        {
            return themeColor;
        }
        return defaultColor;
    }
}

public class BookViewModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string? SeriesTitle { get; set; }
    public int? SeriesNumber { get; set; }
    public DateTime DateAdded { get; set; }
    public bool IsCurrentlyReading { get; set; }
    public double ProgressPercentage { get; set; }
    public string ProgressText { get; set; }
    public string StatusIcon { get; set; }

    public BookViewModel(Book book)
    {
        Id = book.Id;
        Title = book.Title;
        Author = book.AuthorsText;
        SeriesTitle = book.SeriesTitle;
        SeriesNumber = book.SeriesNumber;
        DateAdded = book.DateAdded;
        IsCurrentlyReading = book.IsCurrentlyReading;
        ProgressPercentage = book.ProgressPercentage;
        ProgressText = book.ProgressText;
        
        StatusIcon = book.Status switch
        {
            BookStatus.Reading => "📖",
            BookStatus.Finished => "✅",
            BookStatus.Planned => "📚",
            _ => "📚"
        };
    }
}
