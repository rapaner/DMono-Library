using Library.ViewModels;

namespace Library.Views;

public partial class EditAuthorPage : BasePage
{
    private readonly EditAuthorViewModel _viewModel;

    public EditAuthorPage(EditAuthorViewModel viewModel)
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
