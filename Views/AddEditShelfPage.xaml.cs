using Library.ViewModels;

namespace Library.Views;

public partial class AddEditShelfPage : BasePage
{
    public AddEditShelfPage(AddEditShelfViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
