using Library.Core.Models;

namespace Library.ViewModels;

public class BookItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? SeriesTitle { get; set; }
    public int? SeriesNumber { get; set; }
    public DateTime DateAdded { get; set; }
    public bool IsCurrentlyReading { get; set; }
    public double ProgressPercentage { get; set; }
    public string ProgressText { get; set; } = string.Empty;
    public string StatusIcon { get; set; } = string.Empty;

    public BookItemViewModel(Book book)
    {
        Id = book.Id;
        Title = book.Title;
        Author = book.AuthorsText;
        SeriesTitle = book.SeriesTitle;
        SeriesNumber = book.SeriesNumber;
        DateAdded = book.DateAdded;
        IsCurrentlyReading = book.IsCurrentlyReading;
        ProgressPercentage = book.ProgressPercentage;
        ProgressText = book.ProgressText;

        StatusIcon = book.Status switch
        {
            BookStatus.Reading => "📖",
            BookStatus.Finished => "✅",
            BookStatus.Planned => "📚",
            _ => "📚"
        };
    }
}
