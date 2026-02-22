using Library.ViewModels;

namespace Library.Views;

public partial class BookChoosePage : ContentPage
{
    public BookChoosePage(BookChooseViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
