using Library.Models;
using Library.ViewModels;

namespace Library.Views;

public partial class StatisticsPage : BasePage
{
    private readonly StatisticsViewModel _viewModel;

    public StatisticsPage(StatisticsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SafeExecute(async () => await _viewModel.LoadStatisticsCommand.ExecuteAsync(null));
    }

    private void OnDateFilterChanged(object sender, EventArgs e)
    {
        SafeExecute(async () => await _viewModel.DateFilterChangedCommand.ExecuteAsync(null));
    }

    private void OnCustomDateChanged(object sender, DateChangedEventArgs e)
    {
        SafeExecute(async () => await _viewModel.CustomDateChangedCommand.ExecuteAsync(null));
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        SafeExecute(async () => await _viewModel.SearchChangedCommand.ExecuteAsync(null));
    }

    private void OnBookRankingSelected(object sender, SelectionChangedEventArgs e)
    {
        SafeExecute(async () =>
        {
            if (e.CurrentSelection.FirstOrDefault() is BookRanking selectedRanking)
            {
                await _viewModel.SelectBookRankingCommand.ExecuteAsync(selectedRanking);
            }

            if (sender is CollectionView collectionView)
            {
                collectionView.SelectedItem = null;
            }
        });
    }
}
