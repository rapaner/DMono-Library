using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Controls;
using Library.Core.Models;
using Library.Extensions;
using Library.Helpers;
using Library.Models;
using Library.Services;
using Library.Views;
using System.Collections.ObjectModel;

namespace Library.ViewModels;

public partial class BookDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IBookService _bookService;
    private readonly IStatisticsService _statisticsService;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;
    private Book? _book;
    private int? _selectedSessionId;

    [ObservableProperty]
    private string _bookTitle = string.Empty;

    [ObservableProperty]
    private string _bookAuthor = string.Empty;

    [ObservableProperty]
    private string _totalPages = string.Empty;

    [ObservableProperty]
    private string _statusText = string.Empty;

    [ObservableProperty]
    private string _dateAdded = string.Empty;

    [ObservableProperty]
    private string _dateStarted = string.Empty;

    [ObservableProperty]
    private bool _isDateStartedVisible;

    [ObservableProperty]
    private string _dateFinished = string.Empty;

    [ObservableProperty]
    private bool _isDateFinishedVisible;

    [ObservableProperty]
    private string _seriesTitle = "—";

    [ObservableProperty]
    private string _seriesNumber = "—";

    [ObservableProperty]
    private string _shelfName = "—";

    [ObservableProperty]
    private string _notesText = string.Empty;

    [ObservableProperty]
    private bool _hasNotes;

    [ObservableProperty]
    private bool _isNotesEmpty = true;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private string _progressText = string.Empty;

    [ObservableProperty]
    private int _remainingPages;

    [ObservableProperty]
    private string _remainingPagesText = string.Empty;

    [ObservableProperty]
    private bool _isRemainingPagesVisible;

    [ObservableProperty]
    private string _progressPercentage = string.Empty;

    [ObservableProperty]
    private string _estimatedFinishDate = string.Empty;

    [ObservableProperty]
    private bool _isEstimatedFinishDateVisible;

    [ObservableProperty]
    private bool _isUpdateProgressVisible;

    [ObservableProperty]
    private bool _isReadingScheduleVisible;

    [ObservableProperty]
    private bool _isAlternativeCalculationVisible;

    [ObservableProperty]
    private bool _isRestartReadingVisible;

    [ObservableProperty]
    private bool _isChartVisible;

    [ObservableProperty]
    private bool _isChartHintVisible;

    [ObservableProperty]
    private bool _isReadingSessionsVisible;

    [ObservableProperty]
    private bool _isSessionPickerVisible;

    [ObservableProperty]
    private string _chartDescription = "Страниц прочитано по дням";

    [ObservableProperty]
    private string _averageDailyText = "Среднее количество в день - 0.00";

    [ObservableProperty]
    private ReadingChartDrawable _chartDrawable;

    [ObservableProperty]
    private double _chartWidth = 400;

    [ObservableProperty]
    private ObservableCollection<ReadingSessionDisplayItem> _readingSessions = new();

    private int _bookId;

    public BookDetailViewModel(IBookService bookService, IStatisticsService statisticsService, INavigationService navigation, IDialogService dialog)
    {
        _bookService = bookService;
        _statisticsService = statisticsService;
        _navigation = navigation;
        _dialog = dialog;
        _chartDrawable = new ReadingChartDrawable();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("bookId", out var id))
        {
            if (id is int intId)
                _bookId = intId;
            else if (id is string strId && int.TryParse(strId, out var parsedId))
                _bookId = parsedId;
        }
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        _book = await _bookService.GetBookByIdAsync(_bookId);
        if (_book == null) return;

        LoadBookData();
        await LoadChartData();
    }

    private void LoadBookData()
    {
        if (_book == null) return;

        BookTitle = _book.Title;
        BookAuthor = $"Автор: {_book.AuthorsText}";
        TotalPages = _book.TotalPages.ToString();
        StatusText = _book.StatusText;
        DateAdded = _book.DateAdded.ToString("dd.MM.yyyy");

        LoadReadingSessionsList();
        ApplySelectedSessionDates();

        SeriesTitle = !string.IsNullOrEmpty(_book.SeriesTitle) ? _book.SeriesTitle : "—";
        SeriesNumber = _book.SeriesNumber?.ToString() ?? "—";
        ShelfName = _book.Shelf?.Name ?? "—";

        NotesText = _book.Notes;
        HasNotes = !string.IsNullOrWhiteSpace(NotesText);
        IsNotesEmpty = !HasNotes;

        Progress = _book.ProgressPercentage / 100;
        ProgressText = _book.ProgressText;

        RemainingPages = Math.Max(0, _book.TotalPages - _book.CurrentPage);
        RemainingPagesText = $"Осталось страниц {RemainingPages}";
        IsRemainingPagesVisible = _book.Status == BookStatus.Reading && RemainingPages > 0;

        ProgressPercentage = $"{_book.ProgressPercentage:F2}%";

        bool isFinished = _book.Status == BookStatus.Finished || _book.Status == BookStatus.FinishedLongAgo;
        IsUpdateProgressVisible = !isFinished;
        IsReadingScheduleVisible = !isFinished;
        IsAlternativeCalculationVisible = !isFinished;
        IsRestartReadingVisible = isFinished;
        IsChartVisible = _book.Status != BookStatus.Planned || ReadingSessions.Count > 0;
        IsChartHintVisible = _book.Status == BookStatus.Reading;
    }

    private void LoadReadingSessionsList()
    {
        if (_book == null) return;

        var sessions = _book.ReadingSessions.OrderBy(s => s.Number).ToList();
        IsReadingSessionsVisible = sessions.Count > 0;
        IsSessionPickerVisible = sessions.Count > 1;

        if (_selectedSessionId == null || sessions.All(s => s.Id != _selectedSessionId))
        {
            _selectedSessionId = _book.CurrentSession?.Id ?? sessions.LastOrDefault()?.Id;
        }

        ReadingSessions.Clear();
        foreach (var session in sessions)
        {
            var pages = session.PagesReadHistory.Sum(p => p.PagesRead);
            var days = session.PagesReadHistory.Select(p => p.Date.Date).Distinct().Count();
            var finish = session.FinishDate?.ToString("dd.MM.yyyy") ?? "в процессе";
            var start = session.PagesReadHistory.Any()
                ? session.PagesReadHistory.Min(p => p.Date).ToString("dd.MM.yyyy")
                : session.StartDate.ToString("dd.MM.yyyy");

            ReadingSessions.Add(new ReadingSessionDisplayItem
            {
                SessionId = session.Id,
                Number = session.Number,
                Title = $"Прочтение №{session.Number}",
                DateRangeText = $"{start} — {finish}",
                StatsText = days > 0
                    ? $"{pages} стр., {days} {RussianPluralization.Days(days)}"
                    : $"{pages} стр.",
                IsSelected = session.Id == _selectedSessionId
            });
        }
    }

    private void ApplySelectedSessionDates()
    {
        if (_book == null || _selectedSessionId == null)
        {
            var startDate = _book?.StartDateFromHistory;
            IsDateStartedVisible = startDate.HasValue;
            DateStarted = startDate?.ToString("dd.MM.yyyy") ?? string.Empty;

            var finishedDate = _book?.DateFinished;
            IsDateFinishedVisible = finishedDate.HasValue;
            DateFinished = finishedDate?.ToString("dd.MM.yyyy") ?? string.Empty;
            return;
        }

        var session = _book.ReadingSessions.FirstOrDefault(s => s.Id == _selectedSessionId);
        if (session == null)
            return;

        if (session.PagesReadHistory.Any())
        {
            IsDateStartedVisible = true;
            DateStarted = session.PagesReadHistory.Min(p => p.Date).ToString("dd.MM.yyyy");
        }
        else
        {
            IsDateStartedVisible = true;
            DateStarted = session.StartDate.ToString("dd.MM.yyyy");
        }

        if (session.FinishDate.HasValue)
        {
            IsDateFinishedVisible = true;
            DateFinished = session.FinishDate.Value.ToString("dd.MM.yyyy");
        }
        else
        {
            IsDateFinishedVisible = false;
            DateFinished = string.Empty;
        }
    }

    [RelayCommand]
    private async Task SelectReadingSessionAsync(ReadingSessionDisplayItem? item)
    {
        if (item == null || _book == null) return;

        _selectedSessionId = item.SessionId;
        LoadReadingSessionsList();
        ApplySelectedSessionDates();
        await LoadChartData();
    }

    private async Task LoadChartData()
    {
        if (_book == null) return;

        var dailyData = await _statisticsService.GetDailyReadingDataForBookAsync(
            _book.Id, readingSessionId: _selectedSessionId);

        ChartDrawable.Data = dailyData.Select(d => new Library.Controls.DailyReadingData
        {
            Date = d.Date,
            PagesRead = d.PagesRead
        }).ToList();

        ChartDrawable.PrimaryColor = Application.Current.GetThemeColor("PrimaryColor", Colors.Purple);
        ChartDrawable.TextColor = Application.Current.GetThemeColor("PrimaryTextColor", Colors.Black);
        ChartDrawable.GridColor = Application.Current.GetThemeColor("SecondaryTextColor", Colors.Gray).WithAlpha(0.3f);

        int daysCount = dailyData.Count;
        ChartWidth = daysCount > 0 ? Math.Max(daysCount * 30, 400) : 400;

        if (daysCount > 0)
        {
            var totalPagesRead = dailyData.Sum(d => d.PagesRead);
            var averagePages = (double)totalPagesRead / daysCount;
            ChartDescription = $"Прочитано {totalPagesRead} страниц за {daysCount} {RussianPluralization.Days(daysCount)}";
            AverageDailyText = $"Среднее количество в день - {averagePages:F2}";

            if (_book.Status == BookStatus.Reading && averagePages > 0 && _selectedSessionId == _book.CurrentSession?.Id)
            {
                if (RemainingPages > 0)
                {
                    var daysRemaining = (int)Math.Ceiling(RemainingPages / averagePages);
                    var referenceDate = DateTime.Today;
                    var estimatedDate = referenceDate.AddDays(daysRemaining);

                    EstimatedFinishDate = $"📅 Планируемая дата окончания: {estimatedDate:dd.MM.yyyy}";
                    IsEstimatedFinishDateVisible = true;
                }
                else
                {
                    IsEstimatedFinishDateVisible = false;
                }
            }
            else
            {
                IsEstimatedFinishDateVisible = false;
            }
        }
        else
        {
            ChartDescription = "Нет данных о чтении";
            AverageDailyText = "Среднее количество в день - 0.00";
            IsEstimatedFinishDateVisible = false;
        }

        OnPropertyChanged(nameof(ChartDrawable));
    }

    [RelayCommand]
    private async Task RestartReadingAsync()
    {
        if (_book == null) return;

        bool result = await _dialog.ShowConfirmAsync("Перечитать книгу",
            $"Книга «{_book.Title}» будет переведена в статус «В планах». История прошлых прочтений сохранится. Начать перечитывание?",
            "Да", "Нет");

        if (!result) return;

        await _bookService.RestartReadingAsync(_book.Id);
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task UpdateProgressAsync()
    {
        if (_book == null) return;
        await _navigation.GoToAsync($"{nameof(UpdateProgressPage)}?bookId={_book.Id}");
    }

    [RelayCommand]
    private async Task EditBookAsync()
    {
        if (_book == null) return;
        await _navigation.GoToAsync($"{nameof(AddEditBookPage)}?bookId={_book.Id}");
    }

    [RelayCommand]
    private async Task OpenEditNotesAsync()
    {
        if (_book == null) return;
        await _navigation.GoToAsync($"{nameof(EditBookNotesPage)}?bookId={_book.Id}");
    }

    [RelayCommand]
    private async Task DeleteBookAsync()
    {
        if (_book == null) return;

        bool result = await _dialog.ShowConfirmAsync("Подтверждение",
            $"Вы уверены, что хотите удалить книгу \"{_book.Title}\"?",
            "Да", "Нет");

        if (result)
        {
            await _bookService.DeleteBookAsync(_book);
            await _navigation.GoBackAsync();
        }
    }

    [RelayCommand]
    private async Task GoToReadingScheduleAsync()
    {
        if (_book == null) return;
        await _navigation.GoToAsync($"{nameof(ReadingSchedulePage)}?bookId={_book.Id}");
    }

    [RelayCommand]
    private async Task GoToAlternativeCalculationAsync()
    {
        if (_book == null) return;
        await _navigation.GoToAsync($"{nameof(AlternativePageCalculationPage)}?bookId={_book.Id}");
    }

    [RelayCommand]
    private async Task OpenReadingHistoryAsync()
    {
        if (_book == null || _book.Status != BookStatus.Reading) return;
        await _navigation.GoToAsync($"{nameof(ReadingHistoryEditPage)}?bookId={_book.Id}");
    }
}
