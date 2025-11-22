using Library.Core.Models;
using Library.Services;
using Library.Controls;

namespace Library.Views;

public partial class BookDetailPage : ContentPage
{
    private readonly LibraryService _libraryService;
    private readonly ReadingChartDrawable _chartDrawable;
    private Book _book;

    public BookDetailPage(Book book, LibraryService libraryService)
    {
        InitializeComponent();
        _libraryService = libraryService;
        _book = book;
        
        // Инициализация графика
        _chartDrawable = new ReadingChartDrawable();
        ReadingChartView.Drawable = _chartDrawable;
        
        LoadBookData();
        _ = LoadChartData();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Перезагружаем данные книги из базы данных
        var updatedBook = await _libraryService.GetBookByIdAsync(_book.Id);
        if (updatedBook != null)
        {
            _book = updatedBook;
            LoadBookData();
            await LoadChartData();
        }
    }

    private void LoadBookData()
    {
        BookTitle.Text = _book.Title;
        BookAuthor.Text = $"Автор: {_book.AuthorsText}";
        
        TotalPagesLabel.Text = _book.TotalPages.ToString();
        StatusLabel.Text = _book.StatusText;
        DateAddedLabel.Text = _book.DateAdded.ToString("dd.MM.yyyy");

        var startDate = _book.StartDateFromHistory;
        if (startDate.HasValue)
        {
            DateStartedLabel.Text = startDate.Value.ToString("dd.MM.yyyy");
            DateStartedLabel.IsVisible = true;
            DateStartedCaptionLabel.IsVisible = true;
        }
        else
        {
            DateStartedLabel.IsVisible = false;
            DateStartedCaptionLabel.IsVisible = false;
        }

        var finishedDate = _book.DateFinished;

        if (finishedDate.HasValue)
        {
            DateFinishedLabel.Text = finishedDate.Value.ToString("dd.MM.yyyy");
            DateFinishedLabel.IsVisible = true;
            DateFinishedCaptionLabel.IsVisible = true;
        }
        else
        {
            DateFinishedLabel.IsVisible = false;
            DateFinishedCaptionLabel.IsVisible = false;
        }
        
        // Отображение цикла
        if (!string.IsNullOrEmpty(_book.SeriesTitle))
        {
            SeriesTitleLabel.Text = _book.SeriesTitle;
        }
        else
        {
            SeriesTitleLabel.Text = "—";
        }
        
        // Отображение номера в цикле
        if (_book.SeriesNumber.HasValue)
        {
            SeriesNumberLabel.Text = _book.SeriesNumber.Value.ToString();
        }
        else
        {
            SeriesNumberLabel.Text = "—";
        }
        
        ProgressBar.Progress = _book.ProgressPercentage / 100;
        ProgressText.Text = _book.ProgressText;
        ProgressPercentage.Text = $"{_book.ProgressPercentage:F2}%";
        bool isFinished = _book.Status == BookStatus.Finished;
        UpdateProgressButton.IsVisible = !isFinished;
        ReadingScheduleButton.IsVisible = !isFinished;
        AlternativeCalculationButton.IsVisible = !isFinished;
        
        // Скрыть график для книг "В планах"
        ChartBorder.IsVisible = _book.Status != BookStatus.Planned;
        
        // Показывать подсказку и разрешать редактирование только для книг в статусе "Читаю сейчас"
        bool isReading = _book.Status == BookStatus.Reading;
        ChartHintLabel.IsVisible = isReading;
    }

    private async void OnUpdateProgressClicked(object sender, EventArgs e)
    {
        // Открываем отдельную страницу для обновления прогресса
        await Navigation.PushAsync(new UpdateProgressPage(_book, _libraryService, async () =>
        {
            // Callback для обновления данных после сохранения
            _book = await _libraryService.GetBookByIdAsync(_book.Id) ?? _book;
            LoadBookData();
            await LoadChartData();
        }));
    }

    private async void OnEditBookClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddEditBookPage(_libraryService, _book));
    }

    private async void OnDeleteBookClicked(object sender, EventArgs e)
    {
        bool result = await DisplayAlertAsync("Подтверждение", 
            $"Вы уверены, что хотите удалить книгу \"{_book.Title}\"?", 
            "Да", "Нет");
            
        if (result)
        {
            await _libraryService.DeleteBookAsync(_book);
            await DisplayAlertAsync("Успех", "Книга удалена!", "OK");
            await Navigation.PopAsync();
        }
    }

    private async Task LoadChartData()
    {
        // Загружаем данные о чтении для конкретной книги
        var dailyData = await _libraryService.GetDailyReadingDataForBookAsync(_book.Id);
        
        // Обновляем данные графика
        _chartDrawable.Data = dailyData.Select(d => new Library.Controls.DailyReadingData
        {
            Date = d.Date,
            PagesRead = d.PagesRead
        }).ToList();
        
        // Устанавливаем цвета темы
        _chartDrawable.PrimaryColor = GetThemeColor("PrimaryColor", Colors.Purple);
        _chartDrawable.TextColor = GetThemeColor("PrimaryTextColor", Colors.Black);
        _chartDrawable.GridColor = GetThemeColor("SecondaryTextColor", Colors.Gray).WithAlpha(0.3f);
        
        // Вычисляем ширину графика в зависимости от количества дней
        int daysCount = dailyData.Count;
        if (daysCount > 0)
        {
            // 30 пикселей на день, минимум 400
            ReadingChartView.WidthRequest = Math.Max(daysCount * 30, 400);
        }
        else
        {
            ReadingChartView.WidthRequest = 400;
        }
        
        // Обновляем описание графика
        if (daysCount > 0)
        {
            var totalPages = dailyData.Sum(d => d.PagesRead);
            var averagePages = (double)totalPages / daysCount;
            ChartDescriptionLabel.Text = $"Прочитано {totalPages} страниц за {daysCount} {GetDaysText(daysCount)}";
            AverageDailyLabel.Text = $"Среднее количество в день - {averagePages:F2}";
            
            // Расчет планируемой даты окончания
            if (_book.Status == BookStatus.Reading && averagePages > 0)
            {
                var remainingPages = _book.TotalPages - _book.CurrentPage;
                if (remainingPages > 0)
                {
                    var daysRemaining = (int)Math.Ceiling(remainingPages / averagePages);
                    var lastReadDate = dailyData.Max(d => d.Date);
                    var estimatedFinishDate = lastReadDate.AddDays(daysRemaining);
                    
                    EstimatedFinishDateLabel.Text = $"📅 Планируемая дата окончания: {estimatedFinishDate:dd.MM.yyyy}";
                    EstimatedFinishDateLabel.IsVisible = true;
                }
                else
                {
                    EstimatedFinishDateLabel.IsVisible = false;
                }
            }
            else
            {
                EstimatedFinishDateLabel.IsVisible = false;
            }
        }
        else
        {
            ChartDescriptionLabel.Text = "Нет данных о чтении";
            AverageDailyLabel.Text = "Среднее количество в день - 0.00";
            EstimatedFinishDateLabel.IsVisible = false;
        }
        
        // Перерисовываем график
        ReadingChartView.Invalidate();
    }
    
    private Color GetThemeColor(string resourceKey, Color defaultColor)
    {
        if (Application.Current?.Resources.TryGetValue(resourceKey, out var color) == true && color is Color themeColor)
        {
            return themeColor;
        }
        return defaultColor;
    }
    
    private string GetDaysText(int count)
    {
        var lastDigit = count % 10;
        var lastTwoDigits = count % 100;
        
        if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
            return "дней";
        
        return lastDigit switch
        {
            1 => "день",
            2 or 3 or 4 => "дня",
            _ => "дней"
        };
    }

    /// <summary>
    /// Обработчик нажатия кнопки "График чтения по часам"
    /// </summary>
    private async void OnReadingScheduleClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"{nameof(ReadingSchedulePage)}?bookId={_book.Id}");
    }

    /// <summary>
    /// Обработчик нажатия кнопки "Альтернативный расчет страниц"
    /// </summary>
    private async void OnAlternativePageCalculationClicked(object sender, EventArgs e)
    {
        // Перезагружаем книгу из базы данных для получения актуальных данных
        var updatedBook = await _libraryService.GetBookByIdAsync(_book.Id);
        if (updatedBook != null)
        {
            await Navigation.PushAsync(new AlternativePageCalculationPage(updatedBook, _libraryService));
        }
        else
        {
            await Navigation.PushAsync(new AlternativePageCalculationPage(_book, _libraryService));
        }
    }

    /// <summary>
    /// Обработчик нажатия на график чтения
    /// </summary>
    private async void OnReadingChartTapped(object sender, EventArgs e)
    {
        // Открываем окно редактирования только для книг в статусе "Читаю сейчас"
        if (_book.Status != BookStatus.Reading)
        {
            return;
        }

        // Перезагружаем книгу из базы данных для получения актуальных данных
        var updatedBook = await _libraryService.GetBookByIdAsync(_book.Id);
        if (updatedBook != null)
        {
            await Navigation.PushAsync(new ReadingHistoryEditPage(updatedBook, _libraryService));
        }
        else
        {
            await Navigation.PushAsync(new ReadingHistoryEditPage(_book, _libraryService));
        }
    }
}
