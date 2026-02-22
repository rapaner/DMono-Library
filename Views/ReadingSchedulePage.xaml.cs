using Library.ViewModels;

namespace Library.Views;

public partial class ReadingSchedulePage : ContentPage
{
    public ReadingSchedulePage(ReadingScheduleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
