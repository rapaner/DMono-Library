using Library.ViewModels;

namespace Library.Views;

public partial class LibraryPage : BasePage
{
    private readonly LibraryViewModel _viewModel;

    public LibraryPage(LibraryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SafeExecute(async () => await _viewModel.LoadBooksCommand.ExecuteAsync(null));
    }

    private void OnBookSelected(object sender, SelectionChangedEventArgs e)
    {
        SafeExecute(async () =>
        {
            if (e.CurrentSelection.FirstOrDefault() is BookItemViewModel selectedBook)
            {
                await _viewModel.SelectBookCommand.ExecuteAsync(selectedBook);
            }

            if (sender is CollectionView collectionView)
            {
                collectionView.SelectedItem = null;
            }
        });
    }
}
