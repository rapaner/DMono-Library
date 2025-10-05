using Library.Models;
using Library.Services;

namespace Library.Views;

public partial class BookDetailPage : ContentPage
{
    private readonly LibraryService _libraryService;
    private Book _book;

    public BookDetailPage(Book book, LibraryService libraryService)
    {
        InitializeComponent();
        _libraryService = libraryService;
        _book = book;
        
        LoadBookData();
    }

    private void LoadBookData()
    {
        BookTitle.Text = _book.Title;
        BookAuthor.Text = $"Автор: {_book.AuthorsText}";
        
        TotalPagesLabel.Text = _book.TotalPages.ToString();
        StatusLabel.Text = _book.StatusText;
        DateAddedLabel.Text = _book.DateAdded.ToString("dd.MM.yyyy");
        
        // Отображение цикла
        if (!string.IsNullOrEmpty(_book.SeriesTitle))
        {
            SeriesTitleLabel.Text = _book.SeriesTitle;
        }
        else
        {
            SeriesTitleLabel.Text = "—";
        }
        
        // Отображение номера в цикле
        if (_book.SeriesNumber.HasValue)
        {
            SeriesNumberLabel.Text = _book.SeriesNumber.Value.ToString();
        }
        else
        {
            SeriesNumberLabel.Text = "—";
        }
        
        ProgressBar.Progress = _book.ProgressPercentage / 100;
        ProgressText.Text = _book.ProgressText;
    }

    private async void OnUpdateProgressClicked(object sender, EventArgs e)
    {
        // Открываем отдельную страницу для обновления прогресса
        await Navigation.PushAsync(new UpdateProgressPage(_book, _libraryService, async () =>
        {
            // Callback для обновления данных после сохранения
            _book = await _libraryService.GetBookByIdAsync(_book.Id) ?? _book;
            LoadBookData();
        }));
    }

    private async void OnEditBookClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddEditBookPage(_libraryService, _book));
    }

    private async void OnDeleteBookClicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlert("Подтверждение", 
            $"Вы уверены, что хотите удалить книгу \"{_book.Title}\"?", 
            "Да", "Нет");
            
        if (result)
        {
            await _libraryService.DeleteBookAsync(_book);
            await DisplayAlert("Успех", "Книга удалена!", "OK");
            await Navigation.PopAsync();
        }
    }
}
