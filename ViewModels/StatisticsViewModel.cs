using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Controls;
using Library.Extensions;
using Library.Helpers;
using Library.Models;
using Library.Services;
using Library.Views;
using System.Collections.ObjectModel;

namespace Library.ViewModels;

public partial class StatisticsViewModel : ObservableObject
{
    private readonly IStatisticsService _statisticsService;
    private readonly IDateFilterService _dateFilterService;
    private readonly INavigationService _navigation;

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

    public StatisticsViewModel(IStatisticsService statisticsService, IDateFilterService dateFilterService, INavigationService navigation)
    {
        _statisticsService = statisticsService;
        _dateFilterService = dateFilterService;
        _navigation = navigation;
        _chartDrawable = new ReadingChartDrawable();
    }

    [RelayCommand]
    private async Task LoadStatisticsAsync()
    {
        var (startDate, endDate) = _dateFilterService.GetDateRange(SelectedDateFilterIndex, StartDate, EndDate);
        if (startDate == null && endDate == null && SelectedDateFilterIndex == 6)
            return;

        var searchText = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;
        var statistics = await _statisticsService.GetStatisticsAsync(startDate, endDate, searchText);

        TotalBooks = statistics.TotalBooks.ToString();
        ReadBooks = statistics.ReadBooks.ToString();
        CurrentBooks = statistics.CurrentBooks.ToString();
        PlannedBooks = statistics.PlannedBooks.ToString();
        TotalPagesRead = statistics.TotalPagesRead.ToString();

        PopularAuthors = new ObservableCollection<AuthorStatistic>(statistics.PopularAuthors);

        var bookRankings = await _statisticsService.GetBookRankingsAsync(startDate, endDate, searchText);
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
        var (startDate, endDate) = _dateFilterService.GetDateRange(SelectedDateFilterIndex, StartDate, EndDate);
        await LoadChartDataAsync(startDate, endDate);
    }

    [RelayCommand]
    private async Task SelectBookRankingAsync(BookRanking? ranking)
    {
        if (ranking == null) return;
        await _navigation.GoToAsync($"{nameof(BookDetailPage)}?bookId={ranking.BookId}");
    }

    private async Task LoadChartDataAsync(DateTime? startDate, DateTime? endDate)
    {
        var searchText = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;

        List<Models.DailyReadingData> chartData;
        if (_isMonthlyMode)
            chartData = await _statisticsService.GetMonthlyReadingDataAsync(startDate, endDate, searchText);
        else
            chartData = await _statisticsService.GetDailyReadingDataAsync(startDate, endDate, searchText);

        ChartDrawable.IsMonthlyMode = _isMonthlyMode;
        ChartDrawable.Data = chartData.Select(d => new Library.Controls.DailyReadingData
        {
            Date = d.Date,
            PagesRead = d.PagesRead
        }).ToList();

        ChartDrawable.PrimaryColor = Application.Current.GetThemeColor("PrimaryColor", Colors.Purple);
        ChartDrawable.TextColor = Application.Current.GetThemeColor("PrimaryTextColor", Colors.Black);
        ChartDrawable.GridColor = Application.Current.GetThemeColor("SecondaryTextColor", Colors.Gray).WithAlpha(0.3f);

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
                ChartDescription = $"Прочитано {totalPages} страниц за {dataCount} {RussianPluralization.Months(dataCount)}";
            else
                ChartDescription = $"Прочитано {totalPages} страниц за {dataCount} {RussianPluralization.Days(dataCount)}";

            var dailyData = await _statisticsService.GetDailyReadingDataAsync(startDate, endDate, searchText);
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

}
