namespace Library.Services
{
    /// <summary>
    /// Сервис выбора книги с приоритизацией: меньшие номера имеют больший вес.
    /// </summary>
    internal class PrioritizedRandomBookChooseService : IBookChooseService
    {
        public Task<int> ChooseBook(int booksAmount)
        {
            if (booksAmount <= 0)
            {
                return Task.FromResult(0);
            }

            // Формируем кумулятивный список весов: 1..booksAmount с убывающими весами
            List<KeyValuePair<int, int>> cumulativeWeights = new();
            int currentSum = 0;

            for (int i = 1; i <= booksAmount; i++)
            {
                currentSum += booksAmount - i + 1; // Вес i-й книги: booksAmount - i + 1
                cumulativeWeights.Add(new KeyValuePair<int, int>(currentSum, i));
            }

            int maxValue = cumulativeWeights.Select(x => x.Key).Max();

            // Выполняем booksAmount розыгрышей и выбираем лидера по числу выпадений
            List<int> picks = new(booksAmount);
            for (int i = 0; i < booksAmount; i++)
            {
                int random = Random.Shared.Next(0, maxValue + 1);
                int chosen = cumulativeWeights.First(rl => rl.Key >= random).Value;
                picks.Add(chosen);
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