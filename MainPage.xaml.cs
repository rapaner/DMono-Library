using Library.Models;
using Library.Services;
using Library.Views;

namespace Library;

public partial class MainPage : ContentPage
{
    private readonly LibraryService _libraryService;

    public MainPage(LibraryService libraryService)
    {
        InitializeComponent();
        _libraryService = libraryService;
        
        // Добавляем обработчики нажатий для карточек меню
        AddTapGestures();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await UpdateCurrentBookDisplay();
    }

    private void AddTapGestures()
    {
        // Просмотр библиотеки
        var libraryFrame = (Frame)this.FindByName("libraryFrame");
        var libraryTap = new TapGestureRecognizer();
        libraryTap.Tapped += OnLibraryTapped;
        libraryFrame.GestureRecognizers.Add(libraryTap);

        // Добавить книгу
        var addBookFrame = (Frame)this.FindByName("addBookFrame");
        var addBookTap = new TapGestureRecognizer();
        addBookTap.Tapped += OnAddBookTapped;
        addBookFrame.GestureRecognizers.Add(addBookTap);

        // Статистика
        var statsFrame = (Frame)this.FindByName("statsFrame");
        var statsTap = new TapGestureRecognizer();
        statsTap.Tapped += OnStatsTapped;
        statsFrame.GestureRecognizers.Add(statsTap);

        // Настройки
        var settingsFrame = (Frame)this.FindByName("settingsFrame");
        var settingsTap = new TapGestureRecognizer();
        settingsTap.Tapped += OnSettingsTapped;
        settingsFrame.GestureRecognizers.Add(settingsTap);
    }

    private async Task UpdateCurrentBookDisplay()
    {
        var currentBook = await _libraryService.GetCurrentBookAsync();
        
        if (currentBook != null)
        {
            CurrentBookTitle.Text = currentBook.Title;
            CurrentBookAuthor.Text = $"Автор: {currentBook.Author}";
            CurrentBookProgress.Progress = currentBook.ProgressPercentage / 100;
            CurrentBookProgressText.Text = currentBook.ProgressText;
            ViewCurrentBookButton.IsVisible = true;
        }
        else
        {
            CurrentBookTitle.Text = "Нет активной книги";
            CurrentBookAuthor.Text = "";
            CurrentBookProgress.Progress = 0;
            CurrentBookProgressText.Text = "";
            ViewCurrentBookButton.IsVisible = false;
        }
    }

    private async void OnViewCurrentBookClicked(object sender, EventArgs e)
    {
        var currentBook = await _libraryService.GetCurrentBookAsync();
        if (currentBook != null)
        {
            await Navigation.PushAsync(new BookDetailPage(currentBook, _libraryService));
        }
    }

    private async void OnLibraryTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new LibraryPage(_libraryService));
    }

    private async void OnAddBookTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddEditBookPage(_libraryService));
    }

    private async void OnStatsTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new StatisticsPage(_libraryService));
    }

    private async void OnSettingsTapped(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new SettingsPage(_libraryService));
    }
}
