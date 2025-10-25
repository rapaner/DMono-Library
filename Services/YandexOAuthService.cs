using Library.Models;
using System.Text;

namespace Library.Services
{
    /// <summary>
    /// Сервис для OAuth авторизации в Яндекс
    /// </summary>
    public class YandexOAuthService
    {
        private readonly string _clientId;
        private readonly string _callbackScheme;

        /// <summary>
        /// Конструктор с внедрением зависимости конфигурации
        /// </summary>
        /// <param name="appConfiguration">Конфигурация приложения</param>
        public YandexOAuthService(AppConfiguration appConfiguration)
        {
            _clientId = appConfiguration.YandexOAuthClientId;
            _callbackScheme = appConfiguration.YandexOAuthCallbackScheme;
        }
        
        /// <summary>
        /// Запустить процесс OAuth авторизации для ручного копирования токена
        /// </summary>
        /// <returns>null - пользователь должен скопировать токен из браузера</returns>
        /// <exception cref="InvalidOperationException">Выбрасывается при ошибках конфигурации</exception>
        public async Task<string> AuthenticateAsync()
        {
            try
            {
                // Проверка наличия необходимой конфигурации
                if (string.IsNullOrWhiteSpace(_clientId))
                {
                    var errorMessage1 = "Ошибка OAuth авторизации: Client ID не настроен в конфигурации";
                    System.Diagnostics.Debug.WriteLine(errorMessage1);
                    throw new InvalidOperationException(errorMessage1);
                }

                // Формируем URL для авторизации
                var authUrl = BuildAuthUrl();
                
                // Открываем браузер для авторизации
                // Пользователь должен скопировать токен из открывшегося окна
                await Browser.OpenAsync(authUrl, BrowserLaunchMode.External);

                // Возвращаем null, так как токен нужно вводить вручную
                return null;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Ошибка OAuth авторизации: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(errorMessage);
                throw new InvalidOperationException(errorMessage, ex);
            }
        }

        /// <summary>
        /// Построить URL для OAuth авторизации
        /// </summary>
        private string BuildAuthUrl()
        {
            var sb = new StringBuilder();
            sb.Append("https://oauth.yandex.ru/authorize?");
            sb.Append($"response_type=token");
            sb.Append($"&client_id={_clientId}");
            sb.Append($"&display=popup"); // Отображать токен в popup окне
            
            return sb.ToString();
        }

        /// <summary>
        /// Проверить, поддерживается ли Browser на текущей платформе
        /// </summary>
        public bool IsSupported()
        {
            // Browser поддерживается на всех платформах MAUI
            return true;
        }
    }
}
