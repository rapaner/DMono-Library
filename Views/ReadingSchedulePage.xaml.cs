using Library.ViewModels;

namespace Library.Views;

public partial class ReadingSchedulePage : BasePage
{
    public ReadingSchedulePage(ReadingScheduleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
