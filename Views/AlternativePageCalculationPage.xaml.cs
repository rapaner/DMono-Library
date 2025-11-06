using Library.Core.Models;
using Library.Services;

namespace Library.Views;

public partial class AlternativePageCalculationPage : ContentPage
{
    private readonly LibraryService _libraryService;
    private Book _book;
    private double _mainToAlternativeCoefficient = 1.0;
    private double _alternativeToMainCoefficient = 1.0;
    private bool _isUpdating = false;

    public AlternativePageCalculationPage(Book book, LibraryService libraryService)
    {
        InitializeComponent();
        _libraryService = libraryService;
        _book = book;
        
        LoadBookInfo();
        LoadExistingData();
    }

    private void LoadBookInfo()
    {
        BookTitleLabel.Text = _book.Title;
        BookAuthorLabel.Text = $"Автор: {_book.AuthorsText}";
        TotalPagesLabel.Text = $"Всего страниц: {_book.TotalPages}";
    }

    private void LoadExistingData()
    {
        if (_book.MainFirstPage.HasValue)
        {
            MainFirstPageEntry.Text = _book.MainFirstPage.Value.ToString();
        }
        
        if (_book.AlternativeFirstPage.HasValue)
        {
            AlternativeFirstPageEntry.Text = _book.AlternativeFirstPage.Value.ToString();
        }
        
        if (_book.AlternativeLastPage.HasValue)
        {
            AlternativeLastPageEntry.Text = _book.AlternativeLastPage.Value.ToString();
        }
        
        RecalculateCoefficients();
    }

    private void RecalculateCoefficients()
    {
        if (!int.TryParse(MainFirstPageEntry.Text, out int mainFirst) ||
            !int.TryParse(AlternativeFirstPageEntry.Text, out int altFirst) ||
            !int.TryParse(AlternativeLastPageEntry.Text, out int altLast) ||
            mainFirst < 1 || altFirst < 1 || altLast < altFirst ||
            mainFirst > _book.TotalPages)
        {
            _mainToAlternativeCoefficient = 1.0;
            _alternativeToMainCoefficient = 1.0;
            return;
        }

        int mainLast = _book.TotalPages;
        int mainPages = mainLast - mainFirst + 1;
        int altPages = altLast - altFirst + 1;

        if (mainPages <= 0 || altPages <= 0)
        {
            _mainToAlternativeCoefficient = 1.0;
            _alternativeToMainCoefficient = 1.0;
            return;
        }

        // Коэффициент для перевода из основного в альтернативный
        _mainToAlternativeCoefficient = (double)altPages / mainPages;
        
        // Коэффициент для перевода из альтернативного в основной (обратный)
        _alternativeToMainCoefficient = (double)mainPages / altPages;
    }

    private void OnMainFirstPageChanged(object? sender, TextChangedEventArgs e)
    {
        RecalculateCoefficients();
        UpdateMainPageConversion();
    }

    private void OnAlternativeFirstPageChanged(object? sender, TextChangedEventArgs e)
    {
        RecalculateCoefficients();
        UpdateAlternativePageConversion();
    }

    private void OnAlternativeLastPageChanged(object? sender, TextChangedEventArgs e)
    {
        RecalculateCoefficients();
        UpdateAlternativePageConversion();
    }

    private void OnMainPageChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateMainPageConversion();
    }

    private void OnAlternativePageChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateAlternativePageConversion();
    }

    private void UpdateMainPageConversion()
    {
        if (_isUpdating) return;

        if (string.IsNullOrWhiteSpace(MainPageEntry.Text) ||
            !int.TryParse(MainPageEntry.Text, out int mainPage))
        {
            AlternativePageResultLabel.Text = "—";
            return;
        }

        if (!int.TryParse(MainFirstPageEntry.Text, out int mainFirst) ||
            !int.TryParse(AlternativeFirstPageEntry.Text, out int altFirst))
        {
            AlternativePageResultLabel.Text = "—";
            return;
        }

        // Конвертация: относительная позиция в основном * коэффициент + первая страница альтернативного
        int relativePosition = mainPage - mainFirst;
        double altPage = altFirst + relativePosition * _mainToAlternativeCoefficient;
        
        AlternativePageResultLabel.Text = $"{(int)Math.Round(altPage)}";
    }

    private void UpdateAlternativePageConversion()
    {
        if (_isUpdating) return;

        if (string.IsNullOrWhiteSpace(AlternativePageEntry.Text) ||
            !int.TryParse(AlternativePageEntry.Text, out int altPage))
        {
            MainPageResultLabel.Text = "—";
            return;
        }

        if (!int.TryParse(MainFirstPageEntry.Text, out int mainFirst) ||
            !int.TryParse(AlternativeFirstPageEntry.Text, out int altFirst))
        {
            MainPageResultLabel.Text = "—";
            return;
        }

        // Конвертация: относительная позиция в альтернативном * коэффициент + первая страница основного
        int relativePosition = altPage - altFirst;
        double mainPage = mainFirst + relativePosition * _alternativeToMainCoefficient;
        
        MainPageResultLabel.Text = $"{(int)Math.Round(mainPage)}";
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Валидация
        if (!int.TryParse(MainFirstPageEntry.Text, out int mainFirst) || mainFirst < 1)
        {
            await DisplayAlert("Ошибка", "Введите корректную первую страницу основного издания", "OK");
            return;
        }

        if (mainFirst > _book.TotalPages)
        {
            await DisplayAlert("Ошибка", $"Первая страница основного издания не может быть больше {_book.TotalPages}", "OK");
            return;
        }

        if (!int.TryParse(AlternativeFirstPageEntry.Text, out int altFirst) || altFirst < 1)
        {
            await DisplayAlert("Ошибка", "Введите корректную первую страницу альтернативного издания", "OK");
            return;
        }

        if (!int.TryParse(AlternativeLastPageEntry.Text, out int altLast) || altLast < altFirst)
        {
            await DisplayAlert("Ошибка", "Последняя страница альтернативного издания должна быть больше или равна первой", "OK");
            return;
        }

        try
        {
            // Обновляем значения в книге
            _book.MainFirstPage = mainFirst;
            _book.AlternativeFirstPage = altFirst;
            _book.AlternativeLastPage = altLast;

            // Сохраняем через сервис
            await _libraryService.UpdateBookAsync(_book);
            
            await DisplayAlert("Успех", "Настройки сохранены!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка при сохранении: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

