using Microsoft.Maui.Graphics;

namespace Library.Controls
{
    /// <summary>
    /// Класс для отрисовки графика чтения страниц по дням
    /// </summary>
    public class ReadingChartDrawable : IDrawable
    {
        public List<DailyReadingData> Data { get; set; } = new List<DailyReadingData>();
        public Color PrimaryColor { get; set; } = Colors.Purple;
        public Color TextColor { get; set; } = Colors.Black;
        public Color GridColor { get; set; } = Colors.LightGray;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Data == null || Data.Count == 0)
            {
                DrawEmptyState(canvas, dirtyRect);
                return;
            }

            float padding = 40;
            float chartWidth = dirtyRect.Width - padding * 2;
            float chartHeight = dirtyRect.Height - padding * 2;
            float chartLeft = padding;
            float chartTop = padding;

            // Находим максимальное значение для масштабирования
            int maxPages = Data.Max(d => d.PagesRead);
            if (maxPages == 0) maxPages = 1;

            // Отрисовка сетки и подписей оси Y
            DrawYAxis(canvas, chartLeft, chartTop, chartHeight, maxPages);

            // Отрисовка графика (столбчатая диаграмма)
            DrawBars(canvas, chartLeft, chartTop, chartWidth, chartHeight, maxPages);

            // Отрисовка оси X (даты)
            DrawXAxis(canvas, chartLeft, chartTop, chartWidth, chartHeight);
        }

        private void DrawEmptyState(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FontColor = TextColor;
            canvas.FontSize = 16;
            canvas.DrawString("Нет данных для отображения", 
                dirtyRect.Width / 2, dirtyRect.Height / 2, 
                HorizontalAlignment.Center);
        }

        private void DrawYAxis(ICanvas canvas, float left, float top, float height, int maxPages)
        {
            canvas.StrokeColor = GridColor;
            canvas.StrokeSize = 1;
            canvas.FontColor = TextColor;
            canvas.FontSize = 10;

            // Рисуем 5 горизонтальных линий
            int steps = 5;
            for (int i = 0; i <= steps; i++)
            {
                float y = top + height - (height * i / steps);
                int value = maxPages * i / steps;

                // Горизонтальная линия сетки
                canvas.DrawLine(left, y, left + (Data.Count > 0 ? GetChartWidth(left) : 100), y);

                // Подпись значения
                canvas.DrawString(value.ToString(), left - 5, y, HorizontalAlignment.Right);
            }
        }

        private float GetChartWidth(float left)
        {
            // Ширина графика зависит от количества дней
            return Math.Min(Data.Count * 30f, 800f);
        }

        private void DrawBars(ICanvas canvas, float left, float top, float width, float height, int maxPages)
        {
            float barWidth = Math.Min(width / Data.Count * 0.7f, 40);
            float spacing = width / Data.Count;

            for (int i = 0; i < Data.Count; i++)
            {
                var item = Data[i];
                float barHeight = (float)item.PagesRead / maxPages * height;
                float x = left + spacing * i + spacing / 2 - barWidth / 2;
                float y = top + height - barHeight;

                // Градиент для столбца
                var startColor = PrimaryColor;
                var endColor = Color.FromRgba(PrimaryColor.Red, PrimaryColor.Green, PrimaryColor.Blue, 0.6f);
                
                var paint = new LinearGradientPaint
                {
                    StartPoint = new Point(x, y),
                    EndPoint = new Point(x, y + barHeight),
                    GradientStops = new PaintGradientStop[]
                    {
                        new PaintGradientStop(0, startColor),
                        new PaintGradientStop(1, endColor)
                    }
                };

                canvas.SetFillPaint(paint, new RectF(x, y, barWidth, barHeight));
                canvas.FillRoundedRectangle(x, y, barWidth, barHeight, 4);

                // Значение над столбцом
                if (item.PagesRead > 0)
                {
                    canvas.FontColor = TextColor;
                    canvas.FontSize = 9;
                    canvas.DrawString(item.PagesRead.ToString(), 
                        x + barWidth / 2, y - 5, 
                        HorizontalAlignment.Center);
                }
            }
        }

        private void DrawXAxis(ICanvas canvas, float left, float top, float width, float height)
        {
            canvas.FontColor = TextColor;
            canvas.FontSize = 8;

            float spacing = width / Data.Count;

            for (int i = 0; i < Data.Count; i++)
            {
                var item = Data[i];
                float x = left + spacing * i + spacing / 2;
                float y = top + height + 10;

                // Отображаем дату
                string dateText = item.Date.Day.ToString();
                canvas.DrawString(dateText, x, y, HorizontalAlignment.Center);

                // Отображаем месяц для первого дня месяца или первой записи
                if (item.Date.Day == 1 || i == 0 || item.Date.Month != Data[Math.Max(0, i - 1)].Date.Month)
                {
                    string monthText = GetMonthName(item.Date.Month);
                    canvas.FontSize = 9;
                    canvas.DrawString(monthText, x, y + 12, HorizontalAlignment.Center);
                    canvas.FontSize = 8;
                }
            }
        }

        private string GetMonthName(int month)
        {
            return month switch
            {
                1 => "Янв",
                2 => "Фев",
                3 => "Мар",
                4 => "Апр",
                5 => "Май",
                6 => "Июн",
                7 => "Июл",
                8 => "Авг",
                9 => "Сен",
                10 => "Окт",
                11 => "Ноя",
                12 => "Дек",
                _ => ""
            };
        }
    }

    /// <summary>
    /// Данные о чтении за один день
    /// </summary>
    public class DailyReadingData
    {
        public DateTime Date { get; set; }
        public int PagesRead { get; set; }
    }
}

