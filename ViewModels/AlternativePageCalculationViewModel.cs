using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Core.Models;
using Library.Services;

namespace Library.ViewModels;

public partial class AlternativePageCalculationViewModel : ObservableObject, IQueryAttributable
{
    private readonly LibraryService _libraryService;
    private Book? _book;
    private double _mainToAlternativeCoefficient = 1.0;
    private double _alternativeToMainCoefficient = 1.0;

    [ObservableProperty]
    private string _bookTitleText = string.Empty;

    [ObservableProperty]
    private string _bookAuthorText = string.Empty;

    [ObservableProperty]
    private string _totalPagesText = string.Empty;

    [ObservableProperty]
    private string _mainFirstPageText = string.Empty;

    [ObservableProperty]
    private string _alternativeFirstPageText = string.Empty;

    [ObservableProperty]
    private string _alternativeLastPageText = string.Empty;

    [ObservableProperty]
    private string _mainPageText = string.Empty;

    [ObservableProperty]
    private string _alternativePageText = string.Empty;

    [ObservableProperty]
    private string _alternativePageResult = "—";

    [ObservableProperty]
    private string _mainPageResult = "—";

    public AlternativePageCalculationViewModel(LibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("bookId", out var id))
        {
            int bookId;
            if (id is int intId) bookId = intId;
            else if (id is string strId && int.TryParse(strId, out var parsedId)) bookId = parsedId;
            else return;

            _ = LoadBookAsync(bookId);
        }
    }

    private async Task LoadBookAsync(int bookId)
    {
        _book = await _libraryService.GetBookByIdAsync(bookId);
        if (_book == null) return;

        BookTitleText = _book.Title;
        BookAuthorText = $"Автор: {_book.AuthorsText}";
        TotalPagesText = $"Всего страниц: {_book.TotalPages}";

        if (_book.MainFirstPage.HasValue) MainFirstPageText = _book.MainFirstPage.Value.ToString();
        if (_book.AlternativeFirstPage.HasValue) AlternativeFirstPageText = _book.AlternativeFirstPage.Value.ToString();
        if (_book.AlternativeLastPage.HasValue) AlternativeLastPageText = _book.AlternativeLastPage.Value.ToString();

        RecalculateCoefficients();
    }

    partial void OnMainFirstPageTextChanged(string value) { RecalculateCoefficients(); UpdateMainPageConversion(); }
    partial void OnAlternativeFirstPageTextChanged(string value) { RecalculateCoefficients(); UpdateAlternativePageConversion(); }
    partial void OnAlternativeLastPageTextChanged(string value) { RecalculateCoefficients(); UpdateAlternativePageConversion(); }
    partial void OnMainPageTextChanged(string value) { UpdateMainPageConversion(); }
    partial void OnAlternativePageTextChanged(string value) { UpdateAlternativePageConversion(); }

    private void RecalculateCoefficients()
    {
        if (_book == null ||
            !int.TryParse(MainFirstPageText, out int mainFirst) ||
            !int.TryParse(AlternativeFirstPageText, out int altFirst) ||
            !int.TryParse(AlternativeLastPageText, out int altLast) ||
            mainFirst < 1 || altFirst < 1 || altLast < altFirst ||
            mainFirst > _book.TotalPages)
        {
            _mainToAlternativeCoefficient = 1.0;
            _alternativeToMainCoefficient = 1.0;
            return;
        }

        int mainPages = _book.TotalPages - mainFirst + 1;
        int altPages = altLast - altFirst + 1;

        if (mainPages <= 0 || altPages <= 0)
        {
            _mainToAlternativeCoefficient = 1.0;
            _alternativeToMainCoefficient = 1.0;
            return;
        }

        _mainToAlternativeCoefficient = (double)altPages / mainPages;
        _alternativeToMainCoefficient = (double)mainPages / altPages;
    }

    private void UpdateMainPageConversion()
    {
        if (string.IsNullOrWhiteSpace(MainPageText) || !int.TryParse(MainPageText, out int mainPage) ||
            !int.TryParse(MainFirstPageText, out int mainFirst) || !int.TryParse(AlternativeFirstPageText, out int altFirst))
        {
            AlternativePageResult = "—";
            return;
        }

        int relativePosition = mainPage - mainFirst;
        double altPage = altFirst + relativePosition * _mainToAlternativeCoefficient;
        AlternativePageResult = $"{(int)Math.Round(altPage)}";
    }

    private void UpdateAlternativePageConversion()
    {
        if (string.IsNullOrWhiteSpace(AlternativePageText) || !int.TryParse(AlternativePageText, out int altPage) ||
            !int.TryParse(MainFirstPageText, out int mainFirst) || !int.TryParse(AlternativeFirstPageText, out int altFirst))
        {
            MainPageResult = "—";
            return;
        }

        int relativePosition = altPage - altFirst;
        double mainPage = mainFirst + relativePosition * _alternativeToMainCoefficient;
        MainPageResult = $"{(int)Math.Round(mainPage)}";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_book == null) return;

        if (!int.TryParse(MainFirstPageText, out int mainFirst) || mainFirst < 1)
        {
            await Shell.Current.DisplayAlertAsync("Ошибка", "Введите корректную первую страницу основного издания", "OK");
            return;
        }

        if (mainFirst > _book.TotalPages)
        {
            await Shell.Current.DisplayAlertAsync("Ошибка", $"Первая страница основного издания не может быть больше {_book.TotalPages}", "OK");
            return;
        }

        if (!int.TryParse(AlternativeFirstPageText, out int altFirst) || altFirst < 1)
        {
            await Shell.Current.DisplayAlertAsync("Ошибка", "Введите корректную первую страницу альтернативного издания", "OK");
            return;
        }

        if (!int.TryParse(AlternativeLastPageText, out int altLast) || altLast < altFirst)
        {
            await Shell.Current.DisplayAlertAsync("Ошибка", "Последняя страница альтернативного издания должна быть больше или равна первой", "OK");
            return;
        }

        try
        {
            _book.MainFirstPage = mainFirst;
            _book.AlternativeFirstPage = altFirst;
            _book.AlternativeLastPage = altLast;

            await _libraryService.UpdateBookAsync(_book);

            await Shell.Current.DisplayAlertAsync("Успех", "Настройки сохранены!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Ошибка", $"Произошла ошибка при сохранении: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
