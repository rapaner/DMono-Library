using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Controls;
using Library.Models;
using Library.Services;
using Library.Views;
using System.Collections.ObjectModel;

namespace Library.ViewModels;

public partial class StatisticsViewModel : ObservableObject
{
    private readonly LibraryService _libraryService;

    [ObservableProperty]
    private string _totalBooks = "0";

    [ObservableProperty]
    private string _readBooks = "0";

    [ObservableProperty]
    private string _currentBooks = "0";

    [ObservableProperty]
    private string _plannedBooks = "0";

    [ObservableProperty]
    private string _totalPagesRead = "0";

    [ObservableProperty]
    private string _chartDescription = "Страниц прочитано по дням";

    [ObservableProperty]
    private string _averageDailyText = "Среднее количество в день - 0.00";

    [ObservableProperty]
    private int _selectedDateFilterIndex;

    [ObservableProperty]
    private bool _isCustomDateRangeVisible;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Now.AddDays(-30);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Now;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ReadingChartDrawable _chartDrawable;

    [ObservableProperty]
    private double _chartWidth = 400;

    [ObservableProperty]
    private ObservableCollection<AuthorStatistic> _popularAuthors = new();

    [ObservableProperty]
    private ObservableCollection<BookRanking> _bookRankings = new();

    private bool _isMonthlyMode = true;

    public StatisticsViewModel(LibraryService libraryService)
    {
        _libraryService = libraryService;
        _chartDrawable = new ReadingChartDrawable();
    }

    [RelayCommand]
    private async Task LoadStatisticsAsync()
    {
        var (startDate, endDate) = GetDateRange();
        if (startDate == null && endDate == null && SelectedDateFilterIndex == 6)
            return;

        var searchText = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;
        var statistics = await _libraryService.GetStatisticsAsync(startDate, endDate, searchText);

        TotalBooks = statistics.TotalBooks.ToString();
        ReadBooks = statistics.ReadBooks.ToString();
        CurrentBooks = statistics.CurrentBooks.ToString();
        PlannedBooks = statistics.PlannedBooks.ToString();
        TotalPagesRead = statistics.TotalPagesRead.ToString();

        PopularAuthors = new ObservableCollection<AuthorStatistic>(statistics.PopularAuthors);

        var bookRankings = await _libraryService.GetBookRankingsAsync(startDate, endDate, searchText);
        BookRankings = new ObservableCollection<BookRanking>(bookRankings);

        await LoadChartDataAsync(startDate, endDate);
    }

    [RelayCommand]
    private async Task DateFilterChangedAsync()
    {
        if (SelectedDateFilterIndex == 6)
        {
            IsCustomDateRangeVisible = true;
            EndDate = DateTime.Now;
            StartDate = DateTime.Now.AddDays(-30);
        }
        else
        {
            IsCustomDateRangeVisible = false;
            await LoadStatisticsAsync();
        }
    }

    [RelayCommand]
    private async Task CustomDateChangedAsync()
    {
        await LoadStatisticsAsync();
    }

    [RelayCommand]
    private async Task SearchChangedAsync()
    {
        await LoadStatisticsAsync();
    }

    [RelayCommand]
    private async Task ToggleChartModeAsync()
    {
        _isMonthlyMode = !_isMonthlyMode;
        var (startDate, endDate) = GetDateRange();
        await LoadChartDataAsync(startDate, endDate);
    }

    [RelayCommand]
    private async Task SelectBookRankingAsync(BookRanking? ranking)
    {
        if (ranking == null) return;
        await Shell.Current.GoToAsync($"{nameof(BookDetailPage)}?bookId={ranking.BookId}");
    }

    private async Task LoadChartDataAsync(DateTime? startDate, DateTime? endDate)
    {
        var searchText = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;

        List<Models.DailyReadingData> chartData;
        if (_isMonthlyMode)
            chartData = await _libraryService.GetMonthlyReadingDataAsync(startDate, endDate, searchText);
        else
            chartData = await _libraryService.GetDailyReadingDataAsync(startDate, endDate, searchText);

        ChartDrawable.IsMonthlyMode = _isMonthlyMode;
        ChartDrawable.Data = chartData.Select(d => new Library.Controls.DailyReadingData
        {
            Date = d.Date,
            PagesRead = d.PagesRead
        }).ToList();

        ChartDrawable.PrimaryColor = GetThemeColor("PrimaryColor", Colors.Purple);
        ChartDrawable.TextColor = GetThemeColor("PrimaryTextColor", Colors.Black);
        ChartDrawable.GridColor = GetThemeColor("SecondaryTextColor", Colors.Gray).WithAlpha(0.3f);

        int dataCount = chartData.Count;
        if (dataCount > 0)
        {
            int step = _isMonthlyMode ? 60 : 30;
            ChartWidth = Math.Max(dataCount * step, 400);
        }
        else
        {
            ChartWidth = 400;
        }

        if (dataCount > 0)
        {
            var totalPages = chartData.Sum(d => d.PagesRead);

            if (_isMonthlyMode)
                ChartDescription = $"Прочитано {totalPages} страниц за {dataCount} {GetMonthsText(dataCount)}";
            else
                ChartDescription = $"Прочитано {totalPages} страниц за {dataCount} {GetDaysText(dataCount)}";

            var dailyData = await _libraryService.GetDailyReadingDataAsync(startDate, endDate, searchText);
            if (dailyData.Count > 0)
            {
                var averagePages = (double)dailyData.Sum(d => d.PagesRead) / dailyData.Count;
                AverageDailyText = $"Среднее количество в день - {averagePages:F2}";
            }
            else
            {
                AverageDailyText = "Среднее количество в день - 0.00";
            }
        }
        else
        {
            ChartDescription = "Нет данных о чтении за выбранный период";
            AverageDailyText = "Среднее количество в день - 0.00";
        }

        OnPropertyChanged(nameof(ChartDrawable));
    }

    private (DateTime? start, DateTime? end) GetDateRange()
    {
        return SelectedDateFilterIndex switch
        {
            0 => (null, null),
            1 => (DateTime.Now.AddDays(-30), DateTime.Now),
            2 => (DateTime.Now.AddDays(-60), DateTime.Now),
            3 => (DateTime.Now.AddDays(-90), DateTime.Now),
            4 => (DateTime.Now.AddDays(-180), DateTime.Now),
            5 => (DateTime.Now.AddDays(-365), DateTime.Now),
            6 => (StartDate, EndDate),
            _ => (null, null)
        };
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
        if (lastTwoDigits >= 11 && lastTwoDigits <= 14) return "дней";
        return lastDigit switch { 1 => "день", 2 or 3 or 4 => "дня", _ => "дней" };
    }

    private static string GetMonthsText(int count)
    {
        var lastDigit = count % 10;
        var lastTwoDigits = count % 100;
        if (lastTwoDigits >= 11 && lastTwoDigits <= 14) return "месяцев";
        return lastDigit switch { 1 => "месяц", 2 or 3 or 4 => "месяца", _ => "месяцев" };
    }
}
