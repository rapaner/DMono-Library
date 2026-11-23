namespace Library.Models
{
    /// <summary>
    /// Все данные страницы статистики, рассчитанные за одну загрузку из БД
    /// </summary>
    /// <param name="Statistics">Общая статистика</param>
    /// <param name="Rankings">Рейтинг книг по среднему количеству страниц в день</param>
    /// <param name="DailyData">Количество прочитанных страниц по дням</param>
    public record StatisticsPageData(
        LibraryStatistics Statistics,
        List<BookRanking> Rankings,
        List<DailyReadingData> DailyData);
}
