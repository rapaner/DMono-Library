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
        if (int.TryParse(CurrentPageEntry.Text, out int currentPage))
        {
            if (currentPage >= 0 && currentPage <= _book.TotalPages)
            {
                _book = await _libraryService.UpdateProgressAsync(_book, currentPage);
                
                // Обновить отображение
                ProgressBar.Progress = _book.ProgressPercentage / 100;
                ProgressText.Text = _book.ProgressText;
                StatusLabel.Text = _book.StatusText;
                
                CurrentPageEntry.Text = "";
                
                await DisplayAlert("Успех", "Прогресс обновлен!", "OK");
            }
            else
            {
                await DisplayAlert("Ошибка", "Номер страницы должен быть от 0 до " + _book.TotalPages, "OK");
            }
        }
        else
        {
            await DisplayAlert("Ошибка", "Введите корректный номер страницы", "OK");
        }
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
