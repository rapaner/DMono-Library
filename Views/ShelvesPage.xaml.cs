using Library.ViewModels;

namespace Library.Views;

public partial class ShelvesPage : BasePage
{
    private readonly ShelvesViewModel _viewModel;

    public ShelvesPage(ShelvesViewModel viewModel)
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
