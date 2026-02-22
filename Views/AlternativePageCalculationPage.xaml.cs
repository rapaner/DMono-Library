using Library.ViewModels;

namespace Library.Views;

public partial class AlternativePageCalculationPage : BasePage
{
    public AlternativePageCalculationPage(AlternativePageCalculationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
