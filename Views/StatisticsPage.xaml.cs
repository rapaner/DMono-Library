using Library.Models;
using Library.ViewModels;

namespace Library.Views;

public partial class StatisticsPage : ContentPage
{
    private readonly StatisticsViewModel _viewModel;

    public StatisticsPage(StatisticsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadStatisticsCommand.ExecuteAsync(null);
    }

    private async void OnDateFilterChanged(object sender, EventArgs e)
    {
        await _viewModel.DateFilterChangedCommand.ExecuteAsync(null);
    }

    private async void OnCustomDateChanged(object sender, DateChangedEventArgs e)
    {
        await _viewModel.CustomDateChangedCommand.ExecuteAsync(null);
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        await _viewModel.SearchChangedCommand.ExecuteAsync(null);
    }

    private async void OnBookRankingSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is BookRanking selectedRanking)
        {
            await _viewModel.SelectBookRankingCommand.ExecuteAsync(selectedRanking);
        }

        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }
    }
}
