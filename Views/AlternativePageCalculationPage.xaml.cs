using Library.ViewModels;

namespace Library.Views;

public partial class AlternativePageCalculationPage : ContentPage
{
    public AlternativePageCalculationPage(AlternativePageCalculationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
