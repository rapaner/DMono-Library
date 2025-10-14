using Library.Core.Models;
using Library.Models;
using Library.Services;
using System.Collections.ObjectModel;

namespace Library.Views
{
    /// <summary>
    /// Страница для управления графиком чтения книги по часам
    /// </summary>
    [QueryProperty(nameof(BookId), "bookId")]
    public partial class ReadingSchedulePage : ContentPage
    {
        private readonly LibraryService _libraryService;
        private readonly PageByHourService _pageByHourService;
        private readonly AppConfiguration _appConfig;
        
        private Book? _book;
        private BookReadingSchedule? _schedule;

        public string? BookId { get; set; }

        public ReadingSchedulePage(LibraryService libraryService, PageByHourService pageByHourService, AppConfiguration appConfig)
        {
            InitializeComponent();
            _libraryService = libraryService;
            _pageByHourService = pageByHourService;
            _appConfig = appConfig;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync();
        }

        /// <summary>
        /// Загрузка данных книги и расписания
        /// </summary>
        private async Task LoadDataAsync()
        {
            if (string.IsNullOrEmpty(BookId) || !int.TryParse(BookId, out int bookId))
            {
                await DisplayAlert("Ошибка", "Не указан идентификатор книги", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            try
            {
                LoadingIndicator.IsRunning = true;

                // Загружаем книгу
                _book = await _libraryService.GetBookByIdAsync(bookId);
                if (_book == null)
                {
                    await DisplayAlert("Ошибка", "Книга не найдена", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                // Загружаем расписание
                _schedule = await _libraryService.GetBookReadingScheduleAsync(bookId);

                // Отображаем информацию о книге
                BookTitleLabel.Text = _book.Title;
                BookAuthorLabel.Text = _book.AuthorsText;
                ProgressLabel.Text = $"Прочитано: {_book.CurrentPage} из {_book.TotalPages} страниц";

                // Устанавливаем значения в полях
                if (_schedule != null)
                {
                    FinishDatePicker.Date = _schedule.TargetFinishDate;
                    
                    if (_schedule.StartHour.HasValue)
                        StartHourEntry.Text = _schedule.StartHour.Value.ToString();
                    
                    if (_schedule.EndHour.HasValue)
                        EndHourEntry.Text = _schedule.EndHour.Value.ToString();
                }

                // Устанавливаем минимальную дату (сегодня)
                FinishDatePicker.MinimumDate = DateTime.Today;

                // Обновляем состояние кнопки
                UpdateCalculateButtonState();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
            }
        }

        /// <summary>
        /// Обработчик изменения даты окончания
        /// </summary>
        private void OnFinishDateSelected(object? sender, DateChangedEventArgs e)
        {
            UpdateCalculateButtonState();
        }

        /// <summary>
        /// Обработчик изменения часов
        /// </summary>
        private void OnHoursChanged(object? sender, TextChangedEventArgs e)
        {
            UpdateCalculateButtonState();
        }

        /// <summary>
        /// Обновление состояния кнопки расчета
        /// </summary>
        private void UpdateCalculateButtonState()
        {
            bool isDateValid = FinishDatePicker.Date >= DateTime.Today;
            CalculateButton.IsEnabled = isDateValid;
        }

        /// <summary>
        /// Обработчик нажатия кнопки расчета
        /// </summary>
        private async void OnCalculateClicked(object? sender, EventArgs e)
        {
            if (_book == null)
                return;

            try
            {
                LoadingIndicator.IsRunning = true;
                ScheduleBorder.IsVisible = false;

                // Получаем часы чтения
                int startHour, endHour;
                
                if (int.TryParse(StartHourEntry.Text, out int customStart))
                {
                    startHour = customStart;
                }
                else
                {
                    startHour = _appConfig.DefaultStartHour;
                }

                if (int.TryParse(EndHourEntry.Text, out int customEnd))
                {
                    endHour = customEnd;
                }
                else
                {
                    endHour = _appConfig.DefaultEndHour;
                }

                // Валидация часов
                if (startHour < 0 || startHour > 23 || endHour < 1 || endHour > 24 || startHour >= endHour)
                {
                    await DisplayAlert("Ошибка", "Некорректные часы чтения. Начало должно быть от 0 до 23, окончание от 1 до 24, и начало должно быть меньше окончания.", "OK");
                    return;
                }

                // Сохраняем расписание в БД
                var scheduleToSave = new BookReadingSchedule
                {
                    BookId = _book.Id,
                    TargetFinishDate = FinishDatePicker.Date,
                    StartHour = string.IsNullOrWhiteSpace(StartHourEntry.Text) ? null : startHour,
                    EndHour = string.IsNullOrWhiteSpace(EndHourEntry.Text) ? null : endHour
                };

                await _libraryService.UpdateBookReadingScheduleAsync(scheduleToSave);

                // Выполняем расчет
                int pagesRead = _book.CurrentPage;
                int pagesToRead = _book.TotalPages;
                DateOnly finishDate = DateOnly.FromDateTime(FinishDatePicker.Date);

                var records = await _pageByHourService.Calculate(pagesRead, pagesToRead, finishDate, startHour, endHour);
                var recordsList = records.Take(20).ToList();

                // Отображаем результаты
                if (recordsList.Count > 0)
                {
                    ScheduleCollectionView.ItemsSource = new ObservableCollection<ReadByHourRecord>(recordsList);
                    
                    // Вычисляем сколько страниц в час нужно читать
                    int remainingPages = pagesToRead - pagesRead;
                    decimal pagesPerHour = recordsList.Count > 0 ? (decimal)remainingPages / records.Count() : 0;
                    
                    ScheduleSummaryLabel.Text = $"Осталось прочитать: {remainingPages} страниц\n" +
                                               $"Страниц в час: ~{Math.Ceiling(pagesPerHour)}";
                    
                    ScheduleBorder.IsVisible = true;
                }
                else
                {
                    await DisplayAlert("Внимание", "Недостаточно времени для расчета графика. Попробуйте выбрать более позднюю дату или расширить часы чтения.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось выполнить расчет: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
            }
        }
    }
}

