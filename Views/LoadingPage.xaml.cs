using Library.ViewModels;

namespace Library.Views;

public partial class LoadingPage : BasePage
{
    private readonly LoadingViewModel _viewModel;

    public LoadingPage(LoadingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SafeExecute(async () => await _viewModel.InitializeCommand.ExecuteAsync(null));
    }
}
