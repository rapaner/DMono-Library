using Library.ViewModels;

namespace Library.Views;

public partial class AddEditBookPage : ContentPage
{
    public AddEditBookPage(AddEditBookViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
