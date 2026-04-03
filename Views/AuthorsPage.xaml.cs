using Library.ViewModels;

namespace Library.Views;

public partial class AuthorsPage : BasePage
{
    private readonly AuthorsViewModel _viewModel;

    public AuthorsPage(AuthorsViewModel viewModel)
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
