using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Library.Core.Models;
using Library.Services;
using System.Collections.ObjectModel;

namespace Library.ViewModels;

public partial class ReadingHistoryEditViewModel : ObservableObject, IQueryAttributable
{
    private readonly IBookService _bookService;
    private readonly IReadingProgressService _readingProgressService;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialog;
    private Book? _book;

    public ObservableCollection<ReadingHistoryItemViewModel> Items { get; } = new();

    public ReadingHistoryEditViewModel(IBookService bookService, IReadingProgressService readingProgressService, INavigationService navigation, IDialogService dialog)
    {
        _bookService = bookService;
        _readingProgressService = readingProgressService;
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

            _ = LoadBookAsync(bookId);
        }
    }

    private async Task LoadBookAsync(int bookId)
    {
        _book = await _bookService.GetBookByIdAsync(bookId);
        if (_book != null)
            await LoadHistoryDataAsync();
    }

    private async Task LoadHistoryDataAsync()
    {
        if (_book == null) return;

        Items.Clear();
        var history = await _readingProgressService.GetReadingHistoryAsync(_book.Id);
        var sortedHistory = history.OrderBy(h => h.Date).ToList();
        int runningTotal = 0;

        foreach (var entry in sortedHistory)
        {
            runningTotal += entry.PagesRead;
            Items.Add(new ReadingHistoryItemViewModel
            {
                Date = entry.Date,
                CumulativePages = runningTotal,
                DailyPages = entry.PagesRead
            });
        }
    }

    public void OnCumulativePagesCompleted(ReadingHistoryItemViewModel item, string text)
    {
        if (int.TryParse(text, out int newValue))
            item.CumulativePages = newValue;
        RecalculateDailyPages();
    }

    private void RecalculateDailyPages()
    {
        var sortedItems = Items.OrderBy(i => i.Date).ToList();
        int prevCumulative = 0;
        foreach (var item in sortedItems)
        {
            item.DailyPages = item.CumulativePages - prevCumulative;
            prevCumulative = item.CumulativePages;
        }
    }

    [RelayCommand]
    private void DeleteItem(ReadingHistoryItemViewModel? item)
    {
        if (item != null)
        {
            Items.Remove(item);
            RecalculateDailyPages();
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_book == null) return;

        try
        {
            RecalculateDailyPages();
            var sortedItems = Items.OrderBy(i => i.Date).ToList();

            var invalidItems = sortedItems.Where(i => i.DailyPages <= 0).ToList();
            if (invalidItems.Any())
            {
                await _dialog.ShowAlertAsync("Ошибка валидации",
                    "Нельзя сохранить данные: есть записи с нулевым или отрицательным количеством страниц за день.", "OK");
                return;
            }

            if (sortedItems.Any())
            {
                var maxCumulative = sortedItems.Max(i => i.CumulativePages);
                if (maxCumulative > _book.TotalPages)
                {
                    await _dialog.ShowAlertAsync("Ошибка валидации",
                        $"Сумма прочитанных страниц ({maxCumulative}) не может превышать общее количество страниц в книге ({_book.TotalPages}).", "OK");
                    return;
                }
            }

            if (!sortedItems.Any())
            {
                bool confirm = await _dialog.ShowConfirmAsync("Подтверждение",
                    "Вы собираетесь удалить всю историю чтения. Продолжить?", "Да", "Нет");
                if (!confirm) return;
            }

            var existingHistory = await _readingProgressService.GetReadingHistoryAsync(_book.Id);
            foreach (var entry in existingHistory)
                await _readingProgressService.RemoveReadingProgressAsync(_book.Id, entry.Date);

            foreach (var item in sortedItems)
                await _readingProgressService.AddOrUpdateReadingProgressAsync(_book.Id, item.Date, item.CumulativePages);

            await _dialog.ShowAlertAsync("Успех", "История чтения обновлена", "OK");
            await _navigation.GoBackAsync();
        }
        catch (Exception ex)
        {
            await _dialog.ShowAlertAsync("Ошибка", $"Не удалось сохранить изменения: {ex.Message}", "OK");
        }
    }
}
