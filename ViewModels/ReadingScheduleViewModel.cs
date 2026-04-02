using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Core.Models;
using Library.Models;
using Library.Services;
using System.Collections.ObjectModel;

namespace Library.ViewModels;

public partial class ReadingScheduleViewModel : ObservableObject, IQueryAttributable
{
    private readonly IBookService _bookService;
    private readonly IReadingScheduleService _readingScheduleService;
    private readonly PageByHourService _pageByHourService;
    private readonly AppConfiguration _appConfig;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;

    private Book? _book;
    private BookReadingSchedule? _schedule;

    [ObservableProperty]
    private string _bookTitleText = string.Empty;

    [ObservableProperty]
    private string _bookAuthorText = string.Empty;

    [ObservableProperty]
    private string _progressText = string.Empty;

    [ObservableProperty]
    private DateTime _finishDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _minimumDate = DateTime.Today;

    [ObservableProperty]
    private string _startHourText = string.Empty;

    [ObservableProperty]
    private string _endHourText = string.Empty;

    [ObservableProperty]
    private bool _isCalculateEnabled = true;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isScheduleVisible;

    [ObservableProperty]
    private string _scheduleSummary = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ReadByHourRecord> _scheduleRecords = new();

    public ReadingScheduleViewModel(IBookService bookService, IReadingScheduleService readingScheduleService, PageByHourService pageByHourService, AppConfiguration appConfig, INavigationService navigation, IDialogService dialog)
    {
        _bookService = bookService;
        _readingScheduleService = readingScheduleService;
        _pageByHourService = pageByHourService;
        _appConfig = appConfig;
        _navigation = navigation;
        _dialog = dialog;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("bookId", out var id))
        {
            int bookId;
            if (id is int intId) bookId = intId;
            else if (id is string strId && int.TryParse(strId, out var parsedId)) bookId = parsedId;
            else return;

            _ = LoadDataAsync(bookId);
        }
    }

    private async Task LoadDataAsync(int bookId)
    {
        try
        {
            IsLoading = true;

            _book = await _bookService.GetBookByIdAsync(bookId);
            if (_book == null)
            {
                await _dialog.ShowAlertAsync("Ошибка", "Книга не найдена", "OK");
                await _navigation.GoBackAsync();
                return;
            }

            _schedule = await _readingScheduleService.GetBookReadingScheduleAsync(bookId);

            BookTitleText = _book.Title;
            BookAuthorText = _book.AuthorsText;
            ProgressText = $"Прочитано: {_book.CurrentPage} из {_book.TotalPages} страниц";

            if (_schedule != null)
            {
                FinishDate = _schedule.TargetFinishDate;
                if (_schedule.StartHour.HasValue) StartHourText = _schedule.StartHour.Value.ToString();
                if (_schedule.EndHour.HasValue) EndHourText = _schedule.EndHour.Value.ToString();
            }

            MinimumDate = DateTime.Today;
        }
        catch (Exception ex)
        {
            await _dialog.ShowAlertAsync("Ошибка", $"Не удалось загрузить данные: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CalculateAsync()
    {
        if (_book == null) return;

        try
        {
            IsLoading = true;
            IsScheduleVisible = false;

            int startHour = int.TryParse(StartHourText, out int s) ? s : _appConfig.DefaultStartHour;
            int endHour = int.TryParse(EndHourText, out int e) ? e : _appConfig.DefaultEndHour;

            if (startHour < 0 || startHour > 23 || endHour < 0 || endHour > 23 || startHour >= endHour)
            {
                await _dialog.ShowAlertAsync("Ошибка",
                    "Некорректные часы чтения. Начало должно быть от 0 до 23, окончание от 0 до 23, и начало должно быть меньше окончания.", "OK");
                return;
            }

            var scheduleToSave = new BookReadingSchedule
            {
                BookId = _book.Id,
                TargetFinishDate = FinishDate,
                StartHour = string.IsNullOrWhiteSpace(StartHourText) ? null : startHour,
                EndHour = string.IsNullOrWhiteSpace(EndHourText) ? null : endHour
            };
            await _readingScheduleService.UpdateBookReadingScheduleAsync(scheduleToSave);

            int pagesRead = _book.CurrentPage;
            int pagesToRead = _book.TotalPages;
            DateOnly finishDate = DateOnly.FromDateTime(FinishDate);

            var records = await _pageByHourService.Calculate(pagesRead, pagesToRead, finishDate, startHour, endHour);
            var recordsList = records.Take(20).ToList();

            if (recordsList.Count > 0)
            {
                ScheduleRecords = new ObservableCollection<ReadByHourRecord>(recordsList);

                int remainingPages = pagesToRead - pagesRead;
                decimal pagesPerHour = recordsList.Count > 0 ? (decimal)remainingPages / records.Count() : 0;

                ScheduleSummary = $"Осталось прочитать: {remainingPages} страниц\nСтраниц в час: ~{Math.Ceiling(pagesPerHour)}";
                IsScheduleVisible = true;
            }
            else
            {
                await _dialog.ShowAlertAsync("Внимание",
                    "Недостаточно времени для расчета графика. Попробуйте выбрать более позднюю дату или расширить часы чтения.", "OK");
            }
        }
        catch (Exception ex)
        {
            await _dialog.ShowAlertAsync("Ошибка", $"Не удалось выполнить расчет: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
