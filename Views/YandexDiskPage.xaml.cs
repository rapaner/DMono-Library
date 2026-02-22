using Library.ViewModels;

namespace Library.Views;

public partial class YandexDiskPage : BasePage
{
    private readonly YandexDiskViewModel _viewModel;

    public YandexDiskPage(YandexDiskViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SafeExecute(async () => await _viewModel.LoadSettingsCommand.ExecuteAsync(null));
    }

    private void OnAutoBackupToggled(object sender, ToggledEventArgs e)
    {
        _viewModel.AutoBackupToggledCommand.Execute(e.Value);
    }

    private void OnBackupSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SafeExecute(async () =>
        {
            if (e.CurrentSelection.FirstOrDefault() is YandexDisk.Client.Protocol.Resource backup)
            {
                _viewModel.OnBackupSelected(backup);
                await Shell.Current.DisplayAlert("Резервная копия выбрана",
                    $"Выбрана резервная копия:\n{backup.Name}\n\nНажмите 'Восстановить из резервной копии' для восстановления или 'Удалить резервную копию' для удаления.",
                    "OK");
            }
        });
    }
}
