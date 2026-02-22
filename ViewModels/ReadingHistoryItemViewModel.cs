using System.ComponentModel;

namespace Library.ViewModels;

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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CumulativePages)));
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
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DailyPages)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
