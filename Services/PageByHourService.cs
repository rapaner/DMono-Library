using Library.Core.Models;
using Library.Models;

namespace Library.Services
{
    /// <summary>
    /// Сервис расчета чтения по часам
    /// </summary>
    public class PageByHourService
    {
        /// <summary>
        /// Расчет чтения по часам
        /// </summary>
        /// <param name="pagesRead">Прочитано страниц</param>
        /// <param name="pagesToRead">Нужно прочитать страниц</param>
        /// <param name="finishDate">Дата окончания</param>
        /// <param name="startHour">Стартовый час</param>
        /// <param name="endHour">Последний час</param>
        public Task<IEnumerable<ReadByHourRecord>> Calculate(int pagesRead, int pagesToRead, DateOnly finishDate, int startHour, int endHour)
        {
            List<ReadByHourRecord> records = [];

            DateTime dateNow = DateTime.Now;
            dateNow = dateNow.AddMinutes(-dateNow.Minute).AddSeconds(-dateNow.Second);

            //  Считаем количество часов
            DateTime dateToEnd = finishDate.ToDateTime(TimeOnly.MinValue).AddHours(endHour);

            int count = 0;
            while (dateNow <= dateToEnd)
            {
                if (dateNow.Hour >= startHour && dateNow.Hour <= endHour)
                {
                    count++;
                }
                dateNow = dateNow.AddHours(1);
            }

            //  Проверяем, что есть доступные часы для чтения
            if (count == 0)
            {
                return Task.FromResult(Enumerable.Empty<ReadByHourRecord>());
            }

            //  Считаем разницу между прочитано и надо

            pagesToRead = pagesToRead - pagesRead;
            //  Считаем сколько нужно в час
            decimal pagesForHour = Math.Round((decimal)pagesToRead / count, 2, MidpointRounding.ToZero);

            decimal pagesTemp = pagesRead;

            //  С каждым шагом прибавляем
            dateNow = DateTime.Now;
            dateNow = dateNow.AddMinutes(-dateNow.Minute).AddSeconds(-dateNow.Second).AddHours(1);

            while (count > 0)
            {
                if (dateNow.Hour >= startHour && dateNow.Hour <= endHour)
                {
                    records.Add(new ReadByHourRecord(dateNow, pages: pagesTemp + pagesForHour));
                    pagesTemp += pagesForHour;
                    count--;
                }

                dateNow = dateNow.AddHours(1);
            }

            return Task.FromResult(records.AsEnumerable());
        }
    }
}

