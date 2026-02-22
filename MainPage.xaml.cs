using Library.ViewModels;
using Library.Views;

namespace Library;

public partial class MainPage : BasePage
{
    private readonly MainPageViewModel _viewModel;

    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SafeExecute(async () => await _viewModel.LoadCurrentBookCommand.ExecuteAsync(null));
    }
}
