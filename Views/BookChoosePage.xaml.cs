using Library.Models;
using Library.Services;
using System.Collections.ObjectModel;

namespace Library.Views
{
    public partial class BookChoosePage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SettingsService _settingsService;

        private int _booksAmount;
        private int _lastChosenBookNumber;
        private int _currentBookChooseServiceOption;
        private int _chosenBookNumber;
        private ObservableCollection<BookChooseServiceKey> _bookChooseServiceKeys = new();

        public BookChoosePage(IServiceProvider serviceProvider, SettingsService settingsService)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _settingsService = settingsService;

            // Загружаем настройки
            var (booksAmount, lastChosen, currentOption) = _settingsService.GetBookChooseSettings();
            _booksAmount = booksAmount;
            _lastChosenBookNumber = lastChosen;
            _currentBookChooseServiceOption = currentOption;

            _bookChooseServiceKeys = new ObservableCollection<BookChooseServiceKey>(BookChooseServiceKey.GetAll());

            // Инициализируем UI элементы
            LastChosenBookNumberEntry.Text = _lastChosenBookNumber.ToString();
            BooksAmountEntry.Text = _booksAmount.ToString();
            BookChooseServicePicker.ItemsSource = _bookChooseServiceKeys;
            BookChooseServicePicker.ItemDisplayBinding = new Binding("Name");
            BookChooseServicePicker.SelectedIndex = _currentBookChooseServiceOption >= 0 ? _currentBookChooseServiceOption : -1;
            ChosenBookNumberLabel.Text = _chosenBookNumber > 0 ? _chosenBookNumber.ToString() : "";

            // Подписываемся на изменения
            BooksAmountEntry.TextChanged += OnBooksAmountChanged;
            BookChooseServicePicker.SelectedIndexChanged += OnServiceOptionChanged;
            CalculateButton.Clicked += OnCalculateClicked;
        }

        private void OnBooksAmountChanged(object? sender, TextChangedEventArgs e)
        {
            if (int.TryParse(e.NewTextValue, out int value))
            {
                _booksAmount = value;
            }
        }

        private void OnServiceOptionChanged(object? sender, EventArgs e)
        {
            _currentBookChooseServiceOption = BookChooseServicePicker.SelectedIndex;
        }

        private async void OnCalculateClicked(object? sender, EventArgs e)
        {
            if (_booksAmount <= 0 || _currentBookChooseServiceOption < 0)
            {
                return;
            }

            // Сохраняем текущее состояние настроек до расчёта
            _settingsService.SaveBookChooseSettings(_booksAmount, _lastChosenBookNumber, _currentBookChooseServiceOption);

            var chooseService = _serviceProvider.GetRequiredKeyedService<IBookChooseService>(_currentBookChooseServiceOption);
            var result = await chooseService.ChooseBook(_booksAmount).ConfigureAwait(false);

            _chosenBookNumber = result;
            _lastChosenBookNumber = result;

            // Обновляем UI
            ChosenBookNumberLabel.Text = _chosenBookNumber.ToString();
            LastChosenBookNumberEntry.Text = _lastChosenBookNumber.ToString();

            // Сохраняем результат как последний выбор
            _settingsService.SaveBookChooseSettings(_booksAmount, _lastChosenBookNumber, _currentBookChooseServiceOption);
        }
    }
}