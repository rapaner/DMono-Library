using Library.ViewModels;

namespace Library.Views;

public partial class ReadingHistoryEditPage : ContentPage
{
    private readonly ReadingHistoryEditViewModel _viewModel;

    public ReadingHistoryEditPage(ReadingHistoryEditViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private void OnCumulativePagesCompleted(object? sender, EventArgs e)
    {
        if (sender is Entry entry && entry.BindingContext is ReadingHistoryItemViewModel item)
        {
            _viewModel.OnCumulativePagesCompleted(item, entry.Text);
        }
    }

    private void OnCumulativePagesUnfocused(object? sender, FocusEventArgs e)
    {
        OnCumulativePagesCompleted(sender, e);
    }
}
