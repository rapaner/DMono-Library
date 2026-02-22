using Library.ViewModels;

namespace Library.Views;

public partial class AddEditBookPage : BasePage
{
    public AddEditBookPage(AddEditBookViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
