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
    private string _wontReadBooks = "0";

    [ObservableProperty]
    private string _totalPagesRead = "0";

    [ObservableProperty]
    private string _totalReadings = "0";

    [ObservableProperty]
    private string _rereadBooks = "0";

    [ObservableProperty]
    private string _chartDescription = "Страниц прочитано по дням";

    [ObservableProperty]
    private string _averageDailyText = "Среднее количество в день - 0.00";

    [ObservableProperty]
    private int _selectedDateFilterIndex = 1;

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

    private List<Models.DailyReadingData> _dailyData = new();

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
        var pageData = await _statisticsService.GetStatisticsPageDataAsync(startDate, endDate, searchText);
        var statistics = pageData.Statistics;

        TotalBooks = statistics.TotalBooks.ToString();
        ReadBooks = statistics.ReadBooks.ToString();
        CurrentBooks = statistics.CurrentBooks.ToString();
        PlannedBooks = statistics.PlannedBooks.ToString();
        WontReadBooks = statistics.WontReadBooks.ToString();
        TotalPagesRead = statistics.TotalPagesRead.ToString();
        TotalReadings = statistics.TotalReadings.ToString();
        RereadBooks = statistics.RereadBooks.ToString();

        PopularAuthors = new ObservableCollection<AuthorStatistic>(statistics.PopularAuthors);

        BookRankings = new ObservableCollection<BookRanking>(pageData.Rankings);

        _dailyData = pageData.DailyData;
        UpdateChart();
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
    private void ToggleChartMode()
    {
        _isMonthlyMode = !_isMonthlyMode;
        UpdateChart();
    }

    [RelayCommand]
    private async Task SelectBookRankingAsync(BookRanking? ranking)
    {
        if (ranking == null) return;
        await _navigation.GoToAsync($"{nameof(BookDetailPage)}?bookId={ranking.BookId}");
    }

    private void UpdateChart()
    {
        var dailyData = _dailyData;

        List<Models.DailyReadingData> chartData = _isMonthlyMode
            ? dailyData
                .GroupBy(d => new { d.Date.Year, d.Date.Month })
                .Select(g => new Models.DailyReadingData
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    PagesRead = g.Sum(d => d.PagesRead)
                })
                .OrderBy(d => d.Date)
                .ToList()
            : dailyData;

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
