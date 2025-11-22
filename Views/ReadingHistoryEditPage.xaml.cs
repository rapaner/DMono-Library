using Library.Core.Models;
using Library.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Library.Views;

public partial class ReadingHistoryEditPage : ContentPage
{
    private readonly LibraryService _libraryService;
    private readonly Book _book;
    private readonly ObservableCollection<ReadingHistoryItemViewModel> _items;

    public ReadingHistoryEditPage(Book book, LibraryService libraryService)
    {
        InitializeComponent();
        _libraryService = libraryService;
        _book = book;
        _items = new ObservableCollection<ReadingHistoryItemViewModel>();
        
        HistoryCollectionView.ItemsSource = _items;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadHistoryDataAsync();
    }

    /// <summary>
    /// Загрузка истории чтения из базы данных
    /// </summary>
    private async Task LoadHistoryDataAsync()
    {
        _items.Clear();
        
        var history = await _libraryService.GetReadingHistoryAsync(_book.Id);
        
        // Сортируем по дате и вычисляем накопительную сумму
        var sortedHistory = history.OrderBy(h => h.Date).ToList();
        int runningTotal = 0;
        
        foreach (var entry in sortedHistory)
        {
            runningTotal += entry.PagesRead;
            _items.Add(new ReadingHistoryItemViewModel
            {
                Date = entry.Date,
                CumulativePages = runningTotal,
                DailyPages = entry.PagesRead
            });
        }
    }

    /// <summary>
    /// Обработчик завершения редактирования суммы страниц на дату
    /// </summary>
    private void OnCumulativePagesCompleted(object? sender, EventArgs e)
    {
        if (sender is Entry entry && entry.BindingContext is ReadingHistoryItemViewModel item)
        {
            // Проверяем и обновляем значение, если оно валидно
            if (int.TryParse(entry.Text, out int newValue))
            {
                item.CumulativePages = newValue;
            }
            else
            {
                // Если значение невалидно, восстанавливаем старое значение
                entry.Text = item.CumulativePages.ToString();
            }
            RecalculateDailyPages();
        }
    }

    /// <summary>
    /// Обработчик потери фокуса поля суммы страниц
    /// </summary>
    private void OnCumulativePagesUnfocused(object? sender, FocusEventArgs e)
    {
        OnCumulativePagesCompleted(sender, e);
    }

    /// <summary>
    /// Пересчет количества страниц за день для всех записей
    /// </summary>
    private void RecalculateDailyPages()
    {
        var sortedItems = _items.OrderBy(i => i.Date).ToList();
        int prevCumulative = 0;
        
        foreach (var item in sortedItems)
        {
            item.DailyPages = item.CumulativePages - prevCumulative;
            prevCumulative = item.CumulativePages;
        }
    }

    /// <summary>
    /// Обработчик удаления записи
    /// </summary>
    private void OnDeleteItemClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is ReadingHistoryItemViewModel item)
        {
            _items.Remove(item);
            RecalculateDailyPages();
        }
    }

    /// <summary>
    /// Обработчик сохранения изменений
    /// </summary>
    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        try
        {
            // Пересчитываем значения перед валидацией
            RecalculateDailyPages();
            
            // Валидация данных
            var sortedItems = _items.OrderBy(i => i.Date).ToList();
            
            // Проверка на отрицательные или нулевые значения за день
            var invalidItems = sortedItems.Where(i => i.DailyPages <= 0).ToList();
            if (invalidItems.Any())
            {
                await DisplayAlertAsync("Ошибка валидации", 
                    "Нельзя сохранить данные: есть записи с нулевым или отрицательным количеством страниц за день.", 
                    "OK");
                return;
            }
            
            // Проверка, что последняя сумма не превышает общее количество страниц
            if (sortedItems.Any())
            {
                var maxCumulative = sortedItems.Max(i => i.CumulativePages);
                if (maxCumulative > _book.TotalPages)
                {
                    await DisplayAlertAsync("Ошибка валидации", 
                        $"Сумма прочитанных страниц ({maxCumulative}) не может превышать общее количество страниц в книге ({_book.TotalPages}).", 
                        "OK");
                    return;
                }
            }
            
            // Подтверждение, если история будет полностью очищена
            if (!sortedItems.Any())
            {
                bool confirm = await DisplayAlertAsync("Подтверждение", 
                    "Вы собираетесь удалить всю историю чтения. Продолжить?", 
                    "Да", "Нет");
                if (!confirm)
                {
                    return;
                }
            }
            
            // Сохранение изменений в базу данных
            // Сначала удаляем все существующие записи
            var existingHistory = await _libraryService.GetReadingHistoryAsync(_book.Id);
            foreach (var entry in existingHistory)
            {
                await _libraryService.RemoveReadingProgressAsync(_book.Id, entry.Date);
            }
            
            // Затем создаем новые записи в хронологическом порядке
            foreach (var item in sortedItems)
            {
                await _libraryService.AddOrUpdateReadingProgressAsync(_book.Id, item.Date, item.CumulativePages);
            }
            
            await DisplayAlertAsync("Успех", "История чтения обновлена", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Ошибка", $"Не удалось сохранить изменения: {ex.Message}", "OK");
        }
    }
}

/// <summary>
/// ViewModel для элемента истории чтения
/// </summary>
public class ReadingHistoryItemViewModel : INotifyPropertyChanged
{
    private int _cumulativePages;
    private int _dailyPages;

    public DateTime Date { get; set; }

    public int CumulativePages
    {
        get => _cumulativePages;
        set
        {
            if (_cumulativePages != value)
            {
                _cumulativePages = value;
                OnPropertyChanged(nameof(CumulativePages));
            }
        }
    }

    public int DailyPages
    {
        get => _dailyPages;
        set
        {
            if (_dailyPages != value)
            {
                _dailyPages = value;
                OnPropertyChanged(nameof(DailyPages));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

