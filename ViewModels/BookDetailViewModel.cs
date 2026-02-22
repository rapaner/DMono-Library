using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Controls;
using Library.Core.Models;
using Library.Services;
using Library.Views;

namespace Library.ViewModels;

public partial class BookDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IBookService _bookService;
    private readonly IStatisticsService _statisticsService;
    private Book? _book;

    [ObservableProperty]
    private string _bookTitle = string.Empty;

    [ObservableProperty]
    private string _bookAuthor = string.Empty;

    [ObservableProperty]
    private string _totalPages = string.Empty;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private string _dateAdded = string.Empty;

    [ObservableProperty]
    private string _dateStarted = string.Empty;

    [ObservableProperty]
    private bool _isDateStartedVisible;

    [ObservableProperty]
    private string _dateFinished = string.Empty;

    [ObservableProperty]
    private bool _isDateFinishedVisible;

    [ObservableProperty]
    private string _seriesTitle = "—";

    [ObservableProperty]
    private string _seriesNumber = "—";

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private string _progressText = string.Empty;

    [ObservableProperty]
    private string _progressPercentage = string.Empty;

    [ObservableProperty]
    private string _estimatedFinishDate = string.Empty;

    [ObservableProperty]
    private bool _isEstimatedFinishDateVisible;

    [ObservableProperty]
    private bool _isUpdateProgressVisible;

    [ObservableProperty]
    private bool _isReadingScheduleVisible;

    [ObservableProperty]
    private bool _isAlternativeCalculationVisible;

    [ObservableProperty]
    private bool _isChartVisible;

    [ObservableProperty]
    private bool _isChartHintVisible;

    [ObservableProperty]
    private string _chartDescription = "Страниц прочитано по дням";

    [ObservableProperty]
    private string _averageDailyText = "Среднее количество в день - 0.00";

    [ObservableProperty]
    private ReadingChartDrawable _chartDrawable;

    [ObservableProperty]
    private double _chartWidth = 400;

    private int _bookId;

    public BookDetailViewModel(IBookService bookService, IStatisticsService statisticsService)
    {
        _bookService = bookService;
        _statisticsService = statisticsService;
        _chartDrawable = new ReadingChartDrawable();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("bookId", out var id))
        {
            if (id is int intId)
                _bookId = intId;
            else if (id is string strId && int.TryParse(strId, out var parsedId))
                _bookId = parsedId;
        }
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        _book = await _bookService.GetBookByIdAsync(_bookId);
        if (_book == null) return;

        LoadBookData();
        await LoadChartData();
    }

    private void LoadBookData()
    {
        if (_book == null) return;

        BookTitle = _book.Title;
        BookAuthor = $"Автор: {_book.AuthorsText}";
        TotalPages = _book.TotalPages.ToString();
        StatusText = _book.StatusText;
        DateAdded = _book.DateAdded.ToString("dd.MM.yyyy");

        var startDate = _book.StartDateFromHistory;
        IsDateStartedVisible = startDate.HasValue;
        DateStarted = startDate?.ToString("dd.MM.yyyy") ?? string.Empty;

        var finishedDate = _book.DateFinished;
        IsDateFinishedVisible = finishedDate.HasValue;
        DateFinished = finishedDate?.ToString("dd.MM.yyyy") ?? string.Empty;

        SeriesTitle = !string.IsNullOrEmpty(_book.SeriesTitle) ? _book.SeriesTitle : "—";
        SeriesNumber = _book.SeriesNumber?.ToString() ?? "—";

        Progress = _book.ProgressPercentage / 100;
        ProgressText = _book.ProgressText;
        ProgressPercentage = $"{_book.ProgressPercentage:F2}%";

        bool isFinished = _book.Status == BookStatus.Finished;
        IsUpdateProgressVisible = !isFinished;
        IsReadingScheduleVisible = !isFinished;
        IsAlternativeCalculationVisible = !isFinished;
        IsChartVisible = _book.Status != BookStatus.Planned;
        IsChartHintVisible = _book.Status == BookStatus.Reading;
    }

    private async Task LoadChartData()
    {
        if (_book == null) return;

        var dailyData = await _statisticsService.GetDailyReadingDataForBookAsync(_book.Id);

        ChartDrawable.Data = dailyData.Select(d => new Library.Controls.DailyReadingData
        {
            Date = d.Date,
            PagesRead = d.PagesRead
        }).ToList();

        ChartDrawable.PrimaryColor = GetThemeColor("PrimaryColor", Colors.Purple);
        ChartDrawable.TextColor = GetThemeColor("PrimaryTextColor", Colors.Black);
        ChartDrawable.GridColor = GetThemeColor("SecondaryTextColor", Colors.Gray).WithAlpha(0.3f);

        int daysCount = dailyData.Count;
        ChartWidth = daysCount > 0 ? Math.Max(daysCount * 30, 400) : 400;

        if (daysCount > 0)
        {
            var totalPagesRead = dailyData.Sum(d => d.PagesRead);
            var averagePages = (double)totalPagesRead / daysCount;
            ChartDescription = $"Прочитано {totalPagesRead} страниц за {daysCount} {GetDaysText(daysCount)}";
            AverageDailyText = $"Среднее количество в день - {averagePages:F2}";

            if (_book.Status == BookStatus.Reading && averagePages > 0)
            {
                var remainingPages = _book.TotalPages - _book.CurrentPage;
                if (remainingPages > 0)
                {
                    var daysRemaining = (int)Math.Ceiling(remainingPages / averagePages);
                    var lastReadDate = dailyData.Max(d => d.Date);
                    var estimatedDate = lastReadDate.AddDays(daysRemaining);

                    EstimatedFinishDate = $"📅 Планируемая дата окончания: {estimatedDate:dd.MM.yyyy}";
                    IsEstimatedFinishDateVisible = true;
                }
                else
                {
                    IsEstimatedFinishDateVisible = false;
                }
            }
            else
            {
                IsEstimatedFinishDateVisible = false;
            }
        }
        else
        {
            ChartDescription = "Нет данных о чтении";
            AverageDailyText = "Среднее количество в день - 0.00";
            IsEstimatedFinishDateVisible = false;
        }

        OnPropertyChanged(nameof(ChartDrawable));
    }

    [RelayCommand]
    private async Task UpdateProgressAsync()
    {
        if (_book == null) return;
        await Shell.Current.GoToAsync($"{nameof(UpdateProgressPage)}?bookId={_book.Id}");
    }

    [RelayCommand]
    private async Task EditBookAsync()
    {
        if (_book == null) return;
        await Shell.Current.GoToAsync($"{nameof(AddEditBookPage)}?bookId={_book.Id}");
    }

    [RelayCommand]
    private async Task DeleteBookAsync()
    {
        if (_book == null) return;

        bool result = await Shell.Current.DisplayAlertAsync("Подтверждение",
            $"Вы уверены, что хотите удалить книгу \"{_book.Title}\"?",
            "Да", "Нет");

        if (result)
        {
            await _bookService.DeleteBookAsync(_book);
            await Shell.Current.DisplayAlertAsync("Успех", "Книга удалена!", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    [RelayCommand]
    private async Task GoToReadingScheduleAsync()
    {
        if (_book == null) return;
        await Shell.Current.GoToAsync($"{nameof(ReadingSchedulePage)}?bookId={_book.Id}");
    }

    [RelayCommand]
    private async Task GoToAlternativeCalculationAsync()
    {
        if (_book == null) return;
        await Shell.Current.GoToAsync($"{nameof(AlternativePageCalculationPage)}?bookId={_book.Id}");
    }

    [RelayCommand]
    private async Task OpenReadingHistoryAsync()
    {
        if (_book == null || _book.Status != BookStatus.Reading) return;
        await Shell.Current.GoToAsync($"{nameof(ReadingHistoryEditPage)}?bookId={_book.Id}");
    }

    private static Color GetThemeColor(string resourceKey, Color defaultColor)
    {
        if (Application.Current?.Resources.TryGetValue(resourceKey, out var color) == true && color is Color themeColor)
            return themeColor;
        return defaultColor;
    }

    private static string GetDaysText(int count)
    {
        var lastDigit = count % 10;
        var lastTwoDigits = count % 100;

        if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
            return "дней";

        return lastDigit switch
        {
            1 => "день",
            2 or 3 or 4 => "дня",
            _ => "дней"
        };
    }
}
