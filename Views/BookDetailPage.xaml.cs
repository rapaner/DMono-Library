using Library.ViewModels;

namespace Library.Views;

public partial class BookDetailPage : ContentPage
{
    private readonly BookDetailViewModel _viewModel;

    public BookDetailPage(BookDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
        ReadingChartView.Invalidate();
    }
}
