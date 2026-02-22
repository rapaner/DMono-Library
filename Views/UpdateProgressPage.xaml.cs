using Library.ViewModels;

namespace Library.Views;

public partial class UpdateProgressPage : ContentPage
{
    public UpdateProgressPage(UpdateProgressViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
