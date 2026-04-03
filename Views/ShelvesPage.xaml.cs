using Library.ViewModels;

namespace Library.Views;

public partial class ShelvesPage : BasePage
{
    public ShelvesPage(ShelvesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
