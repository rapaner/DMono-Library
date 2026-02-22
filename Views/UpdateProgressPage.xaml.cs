using Library.ViewModels;

namespace Library.Views;

public partial class UpdateProgressPage : BasePage
{
    public UpdateProgressPage(UpdateProgressViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
