using Library.Models;
using Library.Services;

namespace Library.Views;

public partial class AddEditBookPage : ContentPage
{
    private readonly LibraryService _libraryService;
    private Book? _book;
    private bool _isEditMode;

    public AddEditBookPage(LibraryService libraryService, Book? book = null)
    {
        InitializeComponent();
        _libraryService = libraryService;
        _book = book;
        _isEditMode = book != null;
        
        if (_isEditMode)
        {
            Title = "Редактировать книгу";
            LoadBookData();
        }
    }

    private void LoadBookData()
    {
        if (_book == null) return;
        
        TitleEntry.Text = _book.Title;
        AuthorEntry.Text = _book.Author;
        GenreEntry.Text = _book.Genre;
        TotalPagesEntry.Text = _book.TotalPages.ToString();
        CurrentPageEntry.Text = _book.CurrentPage.ToString();
        RatingSlider.Value = _book.Rating;
        NotesEditor.Text = _book.Notes;
        
        StatusPicker.SelectedIndex = _book.IsCurrentlyReading ? 1 : 
                                    _book.DateFinished.HasValue ? 2 : 0;
    }

    private void OnRatingChanged(object sender, ValueChangedEventArgs e)
    {
        RatingLabel.Text = $"{e.NewValue:F1} ⭐";
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (!ValidateInput())
            return;

        try
        {
            var book = _book ?? new Book();
            
            book.Title = TitleEntry.Text.Trim();
            book.Author = AuthorEntry.Text.Trim();
            book.Genre = GenreEntry.Text.Trim();
            book.TotalPages = int.Parse(TotalPagesEntry.Text);
            book.CurrentPage = string.IsNullOrEmpty(CurrentPageEntry.Text) ? 0 : int.Parse(CurrentPageEntry.Text);
            book.Rating = RatingSlider.Value;
            book.Notes = NotesEditor.Text;
            
            // Установить статус
            var selectedStatus = StatusPicker.SelectedItem?.ToString();
            switch (selectedStatus)
            {
                case "Читаю сейчас":
                    book.IsCurrentlyReading = true;
                    book.DateFinished = null;
                    break;
                case "Прочитано":
                    book.IsCurrentlyReading = false;
                    book.DateFinished = DateTime.Now;
                    break;
                default: // "В планах"
                    book.IsCurrentlyReading = false;
                    book.DateFinished = null;
                    break;
            }
            
            // Если книга помечена как "Читаю сейчас", сбросить флаг у других книг
            if (book.IsCurrentlyReading)
            {
                await _libraryService.SetCurrentBookAsync(book);
            }
            
            if (_isEditMode)
            {
                await _libraryService.UpdateBookAsync(book);
            }
            else
            {
                await _libraryService.AddBookAsync(book);
            }
            
            await DisplayAlert("Успех", 
                _isEditMode ? "Книга обновлена!" : "Книга добавлена!", 
                "OK");
            
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(TitleEntry.Text))
        {
            DisplayAlert("Ошибка", "Пожалуйста, введите название книги", "OK");
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(AuthorEntry.Text))
        {
            DisplayAlert("Ошибка", "Пожалуйста, введите автора", "OK");
            return false;
        }
        
        if (!int.TryParse(TotalPagesEntry.Text, out int totalPages) || totalPages <= 0)
        {
            DisplayAlert("Ошибка", "Пожалуйста, введите корректное количество страниц", "OK");
            return false;
        }
        
        if (!string.IsNullOrEmpty(CurrentPageEntry.Text))
        {
            if (!int.TryParse(CurrentPageEntry.Text, out int currentPage) || 
                currentPage < 0 || currentPage > totalPages)
            {
                DisplayAlert("Ошибка", "Текущая страница должна быть от 0 до " + totalPages, "OK");
                return false;
            }
        }
        
        return true;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
