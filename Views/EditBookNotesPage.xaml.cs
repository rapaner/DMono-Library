using Library.ViewModels;

namespace Library.Views;

public partial class EditBookNotesPage : BasePage
{
    public EditBookNotesPage(EditBookNotesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
