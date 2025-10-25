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
        /// Запустить процесс OAuth авторизации
        /// </summary>
        /// <returns>OAuth токен</returns>
        /// <exception cref="InvalidOperationException">Выбрасывается при ошибках конфигурации или авторизации</exception>
        /// <exception cref="TaskCanceledException">Выбрасывается при отмене авторизации пользователем</exception>
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

                if (string.IsNullOrWhiteSpace(_callbackScheme))
                {
                    var errorMessage2 = "Ошибка OAuth авторизации: Callback Scheme не настроен в конфигурации";
                    System.Diagnostics.Debug.WriteLine(errorMessage2);
                    throw new InvalidOperationException(errorMessage2);
                }

                // Формируем URL для авторизации
                var authUrl = BuildAuthUrl();
                
                // Запускаем WebAuthenticator
                var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                    new Uri(authUrl),
                    new Uri($"{_callbackScheme}://oauth"));

                // Яндекс возвращает токен в fragment (#access_token=...)
                // WebAuthenticator автоматически парсит его
                if (authResult.Properties.TryGetValue("access_token", out var token))
                {
                    return token;
                }

                var errorMessage = "Не удалось получить токен доступа из ответа OAuth";
                System.Diagnostics.Debug.WriteLine(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
            catch (TaskCanceledException)
            {
                // Пользователь отменил авторизацию
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
            sb.Append($"&redirect_uri={Uri.EscapeDataString($"{_callbackScheme}://oauth")}");
            
            return sb.ToString();
        }

        /// <summary>
        /// Проверить, поддерживается ли WebAuthenticator на текущей платформе
        /// </summary>
        public bool IsSupported()
        {
            // WebAuthenticator поддерживается на Android, iOS, macOS, Windows
            return true;
        }
    }
}
