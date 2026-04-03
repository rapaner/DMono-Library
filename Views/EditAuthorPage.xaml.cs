using Library.ViewModels;

namespace Library.Views;

public partial class EditAuthorPage : BasePage
{
    public EditAuthorPage(EditAuthorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
