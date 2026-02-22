using Library.Core.Models;
using Library.Services;
using Library.Controls;
using System.Collections.ObjectModel;
using Library.Models;

namespace Library.Views;

public partial class StatisticsPage : ContentPage
{
    private readonly LibraryService _libraryService;
    private readonly ReadingChartDrawable _chartDrawable;
    private string _searchText = string.Empty;
    private bool _isMonthlyMode = true;

    public StatisticsPage(LibraryService libraryService)
    {
        InitializeComponent();
        _libraryService = libraryService;
        
        // Инициализация графика
        _chartDrawable = new ReadingChartDrawable();
        ReadingChartView.Drawable = _chartDrawable;
        
        // Устанавливаем начальный фильтр "За все время"
        DateFilterPicker.SelectedIndex = 0;
        
        _ = LoadStatistics();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadStatistics();
    }

    private async void OnDateFilterChanged(object sender, EventArgs e)
    {
        if (DateFilterPicker.SelectedIndex == 6) // Произвольно
        {
            CustomDateRangeLayout.IsVisible = true;
            
            // Устанавливаем начальные даты
            EndDatePicker.Date = DateTime.Now;
            StartDatePicker.Date = DateTime.Now.AddDays(-30);
        }
        else
        {
            CustomDateRangeLayout.IsVisible = false;
            await LoadStatistics();
        }
    }

    private async void OnCustomDateChanged(object sender, DateChangedEventArgs e)
    {
        // Загружаем статистику при изменении произвольных дат
        await LoadStatistics();
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _searchText = e.NewTextValue ?? string.Empty;
        await LoadStatistics();
    }

    private async void OnChartTapped(object sender, TappedEventArgs e)
    {
        _isMonthlyMode = !_isMonthlyMode;

        DateTime? startDate = null;
        DateTime? endDate = null;

        switch (DateFilterPicker.SelectedIndex)
        {
            case 0:
                break;
            case 1:
                startDate = DateTime.Now.AddDays(-30);
                endDate = DateTime.Now;
                break;
            case 2:
                startDate = DateTime.Now.AddDays(-60);
                endDate = DateTime.Now;
                break;
            case 3:
                startDate = DateTime.Now.AddDays(-90);
                endDate = DateTime.Now;
                break;
            case 4:
                startDate = DateTime.Now.AddDays(-180);
                endDate = DateTime.Now;
                break;
            case 5:
                startDate = DateTime.Now.AddDays(-365);
                endDate = DateTime.Now;
                break;
            case 6:
                startDate = StartDatePicker.Date;
                endDate = EndDatePicker.Date;
                break;
        }

        await LoadChartData(startDate, endDate);
    }

    private async void OnBookRankingSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is BookRanking selectedRanking)
        {
            var book = await _libraryService.GetBookByIdAsync(selectedRanking.BookId);
            if (book != null)
            {
                await Navigation.PushAsync(new BookDetailPage(book, _libraryService));
            }
        }

        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }
    }

    private async Task LoadStatistics()
    {
        DateTime? startDate = null;
        DateTime? endDate = null;

        // Определяем период на основе выбранного фильтра
        switch (DateFilterPicker.SelectedIndex)
        {
            case 0: // За все время
                startDate = null;
                endDate = null;
                break;
            case 1: // Последние 30 дней
                startDate = DateTime.Now.AddDays(-30);
                endDate = DateTime.Now;
                break;
            case 2: // Последние 60 дней
                startDate = DateTime.Now.AddDays(-60);
                endDate = DateTime.Now;
                break;
            case 3: // Последние 90 дней
                startDate = DateTime.Now.AddDays(-90);
                endDate = DateTime.Now;
                break;
            case 4: // Последние 180 дней
                startDate = DateTime.Now.AddDays(-180);
                endDate = DateTime.Now;
                break;
            case 5: // Последние 365 дней
                startDate = DateTime.Now.AddDays(-365);
                endDate = DateTime.Now;
                break;
            case 6: // Произвольно
                startDate = StartDatePicker.Date;
                endDate = EndDatePicker.Date;
                
                // Проверка правильности дат
                if (startDate > endDate)
                {
                    await DisplayAlertAsync("Ошибка", "Дата начала не может быть больше даты окончания", "OK");
                    return;
                }
                break;
        }

        var searchText = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText;
        var statistics = await _libraryService.GetStatisticsAsync(startDate, endDate, searchText);
        
        // Общая статистика
        TotalBooksLabel.Text = statistics.TotalBooks.ToString();
        ReadBooksLabel.Text = statistics.ReadBooks.ToString();
        CurrentBooksLabel.Text = statistics.CurrentBooks.ToString();
        PlannedBooksLabel.Text = statistics.PlannedBooks.ToString();
        TotalPagesLabel.Text = statistics.TotalPagesRead.ToString();
        
        // Топ авторов
        AuthorsCollectionView.ItemsSource = statistics.PopularAuthors;
        
        // Рейтинг книг
        var bookRankings = await _libraryService.GetBookRankingsAsync(startDate, endDate, searchText);
        BookRankingsCollectionView.ItemsSource = bookRankings;
        
        // Загрузка данных для графика
        await LoadChartData(startDate, endDate);
    }
    
    private async Task LoadChartData(DateTime? startDate, DateTime? endDate)
    {
        var searchText = string.IsNullOrWhiteSpace(_searchText) ? null : _searchText;

        List<Models.DailyReadingData> chartData;
        if (_isMonthlyMode)
        {
            chartData = await _libraryService.GetMonthlyReadingDataAsync(startDate, endDate, searchText);
        }
        else
        {
            chartData = await _libraryService.GetDailyReadingDataAsync(startDate, endDate, searchText);
        }

        _chartDrawable.IsMonthlyMode = _isMonthlyMode;
        _chartDrawable.Data = chartData.Select(d => new Library.Controls.DailyReadingData
        {
            Date = d.Date,
            PagesRead = d.PagesRead
        }).ToList();
        
        _chartDrawable.PrimaryColor = GetThemeColor("PrimaryColor", Colors.Purple);
        _chartDrawable.TextColor = GetThemeColor("PrimaryTextColor", Colors.Black);
        _chartDrawable.GridColor = GetThemeColor("SecondaryTextColor", Colors.Gray).WithAlpha(0.3f);
        
        int dataCount = chartData.Count;
        if (dataCount > 0)
        {
            int step = _isMonthlyMode ? 60 : 30;
            ReadingChartView.WidthRequest = Math.Max(dataCount * step, 400);
        }
        else
        {
            ReadingChartView.WidthRequest = 400;
        }
        
        if (dataCount > 0)
        {
            var totalPages = chartData.Sum(d => d.PagesRead);

            if (_isMonthlyMode)
            {
                ChartDescriptionLabel.Text = $"Прочитано {totalPages} страниц за {dataCount} {GetMonthsText(dataCount)}";
            }
            else
            {
                ChartDescriptionLabel.Text = $"Прочитано {totalPages} страниц за {dataCount} {GetDaysText(dataCount)}";
            }

            var dailyData = await _libraryService.GetDailyReadingDataAsync(startDate, endDate, searchText);
            if (dailyData.Count > 0)
            {
                var averagePages = (double)dailyData.Sum(d => d.PagesRead) / dailyData.Count;
                AverageDailyLabel.Text = $"Среднее количество в день - {averagePages:F2}";
            }
            else
            {
                AverageDailyLabel.Text = "Среднее количество в день - 0.00";
            }
        }
        else
        {
            ChartDescriptionLabel.Text = "Нет данных о чтении за выбранный период";
            AverageDailyLabel.Text = "Среднее количество в день - 0.00";
        }
        
        ReadingChartView.Invalidate();
    }
    
    private Color GetThemeColor(string resourceKey, Color defaultColor)
    {
        if (Application.Current?.Resources.TryGetValue(resourceKey, out var color) == true && color is Color themeColor)
        {
            return themeColor;
        }
        return defaultColor;
    }
    
    private string GetDaysText(int count)
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

    private string GetMonthsText(int count)
    {
        var lastDigit = count % 10;
        var lastTwoDigits = count % 100;

        if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
            return "месяцев";

        return lastDigit switch
        {
            1 => "месяц",
            2 or 3 or 4 => "месяца",
            _ => "месяцев"
        };
    }
}
