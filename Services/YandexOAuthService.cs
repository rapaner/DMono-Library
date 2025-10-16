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
        /// <returns>OAuth токен или null в случае ошибки</returns>
        public async Task<string?> AuthenticateAsync()
        {
            try
            {
                // Проверка наличия необходимой конфигурации
                if (string.IsNullOrWhiteSpace(_clientId))
                {
                    System.Diagnostics.Debug.WriteLine("Ошибка OAuth авторизации: Client ID не настроен в конфигурации");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(_callbackScheme))
                {
                    System.Diagnostics.Debug.WriteLine("Ошибка OAuth авторизации: Callback Scheme не настроен в конфигурации");
                    return null;
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

                return null;
            }
            catch (TaskCanceledException)
            {
                // Пользователь отменил авторизацию
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка OAuth авторизации: {ex.Message}");
                return null;
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
