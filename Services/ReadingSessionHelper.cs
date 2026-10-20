using Library.Core.Data;
using Library.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Services
{
    internal static class ReadingSessionHelper
    {
        public static ReadingSession? GetOpenSession(Book book) =>
            book.ReadingSessions?.FirstOrDefault(s => s.FinishDate == null);

        public static ReadingSession CreateSession(Book book, LibraryDbContext context, DateTime? startDate = null)
        {
            var number = book.ReadingSessions.Count == 0
                ? 1
                : book.ReadingSessions.Max(s => s.Number) + 1;

            var session = new ReadingSession
            {
                BookId = book.Id,
                Number = number,
                StartDate = (startDate ?? DateTime.Now).Date
            };

            book.ReadingSessions.Add(session);
            context.ReadingSessions.Add(session);
            return session;
        }

        public static ReadingSession EnsureOpenSession(Book book, LibraryDbContext context)
        {
            var open = GetOpenSession(book);
            if (open != null)
                return open;

            return CreateSession(book, context);
        }

        public static void CloseSession(ReadingSession session, DateTime? finishDate = null)
        {
            session.FinishDate = (finishDate ?? DateTime.Now).Date;
        }

        public static IQueryable<Book> WithReadingData(IQueryable<Book> query) =>
            query
                .Include(b => b.ReadingSessions)
                    .ThenInclude(s => s.PagesReadHistory);
    }
}
