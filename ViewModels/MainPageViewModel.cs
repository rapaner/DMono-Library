using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Services;
using Library.Views;

namespace Library.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly LibraryService _libraryService;

    [ObservableProperty]
    private string _currentBookTitle = "Нет активной книги";

    [ObservableProperty]
    private string _currentBookAuthor = string.Empty;

    [ObservableProperty]
    private double _currentBookProgress;

    [ObservableProperty]
    private string _currentBookProgressText = string.Empty;

    [ObservableProperty]
    private string _currentBookPercentage = string.Empty;

    [ObservableProperty]
    private bool _isViewCurrentBookVisible;

    private int? _currentBookId;

    public MainPageViewModel(LibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    [RelayCommand]
    private async Task LoadCurrentBookAsync()
    {
        var currentBook = await _libraryService.GetCurrentBookAsync();

        if (currentBook != null)
        {
            _currentBookId = currentBook.Id;
            CurrentBookTitle = currentBook.Title;
            CurrentBookAuthor = $"Автор: {currentBook.AuthorsText}";
            CurrentBookProgress = currentBook.ProgressPercentage / 100;
            CurrentBookProgressText = currentBook.ProgressText;
            CurrentBookPercentage = $"{currentBook.ProgressPercentage:F2}%";
            IsViewCurrentBookVisible = true;
        }
        else
        {
            _currentBookId = null;
            CurrentBookTitle = "Нет активной книги";
            CurrentBookAuthor = string.Empty;
            CurrentBookProgress = 0;
            CurrentBookProgressText = string.Empty;
            CurrentBookPercentage = string.Empty;
            IsViewCurrentBookVisible = false;
        }
    }

    [RelayCommand]
    private async Task ViewCurrentBookAsync()
    {
        if (_currentBookId.HasValue)
        {
            await Shell.Current.GoToAsync($"{nameof(BookDetailPage)}?bookId={_currentBookId.Value}");
        }
    }

    [RelayCommand]
    private async Task GoToLibraryAsync()
    {
        await Shell.Current.GoToAsync(nameof(LibraryPage));
    }

    [RelayCommand]
    private async Task GoToAddBookAsync()
    {
        await Shell.Current.GoToAsync(nameof(AddEditBookPage));
    }

    [RelayCommand]
    private async Task GoToStatisticsAsync()
    {
        await Shell.Current.GoToAsync(nameof(StatisticsPage));
    }

    [RelayCommand]
    private async Task GoToSettingsAsync()
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    [RelayCommand]
    private async Task GoToBookChooseAsync()
    {
        await Shell.Current.GoToAsync(nameof(BookChoosePage));
    }
}
