using Library.ViewModels;

namespace Library.Views;

public partial class YandexDiskPage : ContentPage
{
    private readonly YandexDiskViewModel _viewModel;

    public YandexDiskPage(YandexDiskViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSettingsCommand.ExecuteAsync(null);
    }

    private void OnAutoBackupToggled(object sender, ToggledEventArgs e)
    {
        _viewModel.AutoBackupToggledCommand.Execute(e.Value);
    }

    private async void OnBackupSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is YandexDisk.Client.Protocol.Resource backup)
        {
            _viewModel.OnBackupSelected(backup);
            await Shell.Current.DisplayAlert("Резервная копия выбрана",
                $"Выбрана резервная копия:\n{backup.Name}\n\nНажмите 'Восстановить из резервной копии' для восстановления или 'Удалить резервную копию' для удаления.",
                "OK");
        }
    }
}
