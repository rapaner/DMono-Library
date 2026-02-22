using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Models;
using Library.Services;
using System.Collections.ObjectModel;

namespace Library.ViewModels;

public partial class BookChooseViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private string _booksAmountText = string.Empty;

    [ObservableProperty]
    private string _lastChosenBookNumberText = string.Empty;

    [ObservableProperty]
    private string _chosenBookNumberText = string.Empty;

    [ObservableProperty]
    private int _selectedServiceIndex = -1;

    public ObservableCollection<BookChooseServiceKey> ServiceKeys { get; } = new();

    private int _booksAmount;
    private int _lastChosenBookNumber;
    private int _currentBookChooseServiceOption;

    public BookChooseViewModel(IServiceProvider serviceProvider, SettingsService settingsService)
    {
        _serviceProvider = serviceProvider;
        _settingsService = settingsService;

        var (booksAmount, lastChosen, currentOption) = _settingsService.GetBookChooseSettings();
        _booksAmount = booksAmount;
        _lastChosenBookNumber = lastChosen;
        _currentBookChooseServiceOption = currentOption;

        foreach (var key in BookChooseServiceKey.GetAll())
            ServiceKeys.Add(key);

        LastChosenBookNumberText = _lastChosenBookNumber.ToString();
        BooksAmountText = _booksAmount.ToString();
        SelectedServiceIndex = _currentBookChooseServiceOption >= 0 ? _currentBookChooseServiceOption : -1;
    }

    partial void OnBooksAmountTextChanged(string value)
    {
        if (int.TryParse(value, out int parsed))
            _booksAmount = parsed;
    }

    partial void OnSelectedServiceIndexChanged(int value)
    {
        _currentBookChooseServiceOption = value;
    }

    [RelayCommand]
    private async Task CalculateAsync()
    {
        if (_booksAmount <= 0 || _currentBookChooseServiceOption < 0)
            return;

        _settingsService.SaveBookChooseSettings(_booksAmount, _lastChosenBookNumber, _currentBookChooseServiceOption);

        var chooseService = _serviceProvider.GetRequiredKeyedService<IBookChooseService>(_currentBookChooseServiceOption);
        var result = await chooseService.ChooseBook(_booksAmount).ConfigureAwait(false);

        _lastChosenBookNumber = result;

        ChosenBookNumberText = result.ToString();
        LastChosenBookNumberText = result.ToString();

        _settingsService.SaveBookChooseSettings(_booksAmount, _lastChosenBookNumber, _currentBookChooseServiceOption);
    }
}
