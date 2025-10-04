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
        BookAuthor.Text = $"Автор: {_book.Author}";
        BookGenre.Text = $"Жанр: {_book.Genre}";
        
        TotalPagesLabel.Text = _book.TotalPages.ToString();
        StatusLabel.Text = _book.StatusText;
        DateAddedLabel.Text = _book.DateAdded.ToString("dd.MM.yyyy");
        RatingLabel.Text = _book.Rating > 0 ? $"{_book.Rating:F1} ⭐" : "Не оценено";
        
        ProgressBar.Progress = _book.ProgressPercentage / 100;
        ProgressText.Text = _book.ProgressText;
        
        NotesEditor.Text = _book.Notes;
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

    private async void OnSaveNotesClicked(object sender, EventArgs e)
    {
        _book.Notes = NotesEditor.Text;
        await _libraryService.UpdateBookAsync(_book);
        
        await DisplayAlert("Успех", "Заметки сохранены!", "OK");
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
