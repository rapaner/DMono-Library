using Library.Models;
using Library.Services;
using System.Collections.ObjectModel;

namespace Library.Views;

public partial class StatisticsPage : ContentPage
{
    private readonly LibraryService _libraryService;

    public StatisticsPage(LibraryService libraryService)
    {
        InitializeComponent();
        _libraryService = libraryService;
        
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
                    await DisplayAlert("Ошибка", "Дата начала не может быть больше даты окончания", "OK");
                    return;
                }
                break;
        }

        var statistics = await _libraryService.GetStatisticsAsync(startDate, endDate);
        
        // Общая статистика
        TotalBooksLabel.Text = statistics.TotalBooks.ToString();
        ReadBooksLabel.Text = statistics.ReadBooks.ToString();
        CurrentBooksLabel.Text = statistics.CurrentBooks.ToString();
        PlannedBooksLabel.Text = statistics.PlannedBooks.ToString();
        TotalPagesLabel.Text = statistics.TotalPagesRead.ToString();
        
        // Топ авторов
        AuthorsCollectionView.ItemsSource = statistics.PopularAuthors;
    }
}
