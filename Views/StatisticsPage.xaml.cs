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
        
        _ = LoadStatistics();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadStatistics();
    }

    private async Task LoadStatistics()
    {
        var statistics = await _libraryService.GetStatisticsAsync();
        
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
