using Library.ViewModels;

namespace Library.Views;

public partial class AddEditShelfPage : BasePage
{
    private readonly AddEditShelfViewModel _viewModel;

    public AddEditShelfPage(AddEditShelfViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SafeExecute(async () => await _viewModel.LoadBooksCommand.ExecuteAsync(null));
    }
}
