using Library.ViewModels;

namespace Library.Views;

public partial class BookChoosePage : BasePage
{
    public BookChoosePage(BookChooseViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
