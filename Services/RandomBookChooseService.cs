namespace Library.Services
{
    /// <summary>
    /// Обычный рандомайзер: равновероятный выбор, итог — один номер лидера среди серии попыток.
    /// </summary>
    internal class RandomBookChooseService : IBookChooseService
    {
        public Task<int> ChooseBook(int booksAmount)
        {
            if (booksAmount <= 0)
            {
                return Task.FromResult(0);
            }

            // Выполняем booksAmount равновероятных выборов из диапазона [1..booksAmount]
            List<int> picks = new(booksAmount);
            for (int i = 0; i < booksAmount; i++)
            {
                int value = Random.Shared.Next(1, booksAmount + 1);
                picks.Add(value);
            }

            int chosenNumber = picks
                .GroupBy(x => x)
                .Select(g => new { Number = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Number)
                .First().Number;

            return Task.FromResult(chosenNumber);
        }
    }
}