using Library.Models;
using Library.Services;
using System.Collections.ObjectModel;

namespace Library.Views;

public partial class AddEditBookPage : ContentPage
{
    private readonly LibraryService _libraryService;
    private Book? _book;
    private bool _isEditMode;
    private ObservableCollection<Author> _selectedAuthors;
    private List<Author> _allAuthors;

    public AddEditBookPage(LibraryService libraryService, Book? book = null)
    {
        InitializeComponent();
        _libraryService = libraryService;
        _book = book;
        _isEditMode = book != null;
        _selectedAuthors = new ObservableCollection<Author>();
        _allAuthors = new List<Author>();
        
        if (_isEditMode)
        {
            Title = "Редактировать книгу";
        }
        
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        // Загрузить всех авторов
        _allAuthors = await _libraryService.GetAllAuthorsAsync();
        AuthorPicker.ItemsSource = _allAuthors.Select(a => a.Name).ToList();
        
        if (_isEditMode && _book != null)
        {
            LoadBookData();
        }
    }

    private void LoadBookData()
    {
        if (_book == null) return;
        
        TitleEntry.Text = _book.Title;
        TotalPagesEntry.Text = _book.TotalPages.ToString();
        
        // Загрузить выбранных авторов
        _selectedAuthors.Clear();
        foreach (var author in _book.Authors)
        {
            _selectedAuthors.Add(author);
        }
        SelectedAuthorsCollectionView.ItemsSource = _selectedAuthors;
        
        // Цикл
        if (!string.IsNullOrEmpty(_book.SeriesTitle))
        {
            SeriesTitleEntry.Text = _book.SeriesTitle;
            if (_book.SeriesNumber.HasValue)
            {
                SeriesNumberEntry.Text = _book.SeriesNumber.Value.ToString();
            }
        }
        
        StatusPicker.SelectedIndex = _book.IsCurrentlyReading ? 1 : 
                                    _book.DateFinished.HasValue ? 2 : 0;
    }

    private async void OnAddAuthorClicked(object sender, EventArgs e)
    {
        if (AuthorPicker.SelectedIndex < 0)
        {
            await DisplayAlert("Ошибка", "Выберите автора из списка", "OK");
            return;
        }
        
        var selectedAuthorName = AuthorPicker.SelectedItem as string;
        var author = _allAuthors.FirstOrDefault(a => a.Name == selectedAuthorName);
        
        if (author != null && !_selectedAuthors.Any(a => a.Id == author.Id))
        {
            _selectedAuthors.Add(author);
            SelectedAuthorsCollectionView.ItemsSource = null;
            SelectedAuthorsCollectionView.ItemsSource = _selectedAuthors;
        }
    }

    private async void OnNewAuthorClicked(object sender, EventArgs e)
    {
        var result = await DisplayPromptAsync("Новый автор", "Введите имя автора:", "Добавить", "Отмена");
        
        if (!string.IsNullOrWhiteSpace(result))
        {
            // Проверить, существует ли автор
            var existingAuthor = _allAuthors.FirstOrDefault(a => a.Name == result);
            if (existingAuthor != null)
            {
                await DisplayAlert("Информация", "Такой автор уже существует", "OK");
                return;
            }
            
            // Создать нового автора
            var newAuthor = await _libraryService.AddAuthorAsync(new Author { Name = result });
            _allAuthors.Add(newAuthor);
            _selectedAuthors.Add(newAuthor);
            
            // Обновить список
            AuthorPicker.ItemsSource = _allAuthors.Select(a => a.Name).ToList();
            SelectedAuthorsCollectionView.ItemsSource = null;
            SelectedAuthorsCollectionView.ItemsSource = _selectedAuthors;
            
            await DisplayAlert("Успех", "Автор добавлен!", "OK");
        }
    }

    private void OnRemoveAuthorClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Author author)
        {
            _selectedAuthors.Remove(author);
            SelectedAuthorsCollectionView.ItemsSource = null;
            SelectedAuthorsCollectionView.ItemsSource = _selectedAuthors;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (!ValidateInput())
            return;

        try
        {
            var book = _book ?? new Book();
            
            book.Title = TitleEntry.Text.Trim();
            book.TotalPages = int.Parse(TotalPagesEntry.Text);
            
            // Цикл
            book.SeriesTitle = string.IsNullOrWhiteSpace(SeriesTitleEntry.Text) ? null : SeriesTitleEntry.Text.Trim();
            if (!string.IsNullOrWhiteSpace(SeriesNumberEntry.Text) && int.TryParse(SeriesNumberEntry.Text, out int seriesNumber))
            {
                book.SeriesNumber = seriesNumber;
            }
            else
            {
                book.SeriesNumber = null;
            }
            
            // Авторы
            book.Authors.Clear();
            foreach (var author in _selectedAuthors)
            {
                book.Authors.Add(author);
            }
            
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
        
        if (_selectedAuthors.Count == 0)
        {
            DisplayAlert("Ошибка", "Пожалуйста, выберите хотя бы одного автора", "OK");
            return false;
        }
        
        if (!int.TryParse(TotalPagesEntry.Text, out int totalPages) || totalPages <= 0)
        {
            DisplayAlert("Ошибка", "Пожалуйста, введите корректное количество страниц", "OK");
            return false;
        }
        
        return true;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}