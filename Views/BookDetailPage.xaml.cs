using Library.ViewModels;

namespace Library.Views;

public partial class BookDetailPage : BasePage
{
    private readonly BookDetailViewModel _viewModel;

    public BookDetailPage(BookDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SafeExecute(async () =>
        {
            await _viewModel.LoadDataCommand.ExecuteAsync(null);
            ReadingChartView.Invalidate();
        });
    }
}
