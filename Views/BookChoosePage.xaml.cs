using Library.ViewModels;

namespace Library.Views;

public partial class BookChoosePage : BasePage
{
    private readonly BookChooseViewModel _viewModel;

    public BookChoosePage(BookChooseViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SafeExecute(async () => await _viewModel.LoadDataCommand.ExecuteAsync(null));
    }
}
