using System.Text;

namespace Library.Services
{
    /// <summary>
    /// Сервис для OAuth авторизации в Яндекс
    /// </summary>
    public class YandexOAuthService
    {
        // Client ID для приложения (нужно зарегистрировать на https://oauth.yandex.ru/)
        // Для тестирования можно использовать этот публичный Client ID
        private const string ClientId = "a25f1436a80a4d148cfc17bf422b0dc7";
        
        // Callback URL для получения токена
        private const string CallbackScheme = "ru.rapaner.library";
        
        /// <summary>
        /// Запустить процесс OAuth авторизации
        /// </summary>
        /// <returns>OAuth токен или null в случае ошибки</returns>
        public async Task<string?> AuthenticateAsync()
        {
            try
            {
                // Формируем URL для авторизации
                var authUrl = BuildAuthUrl();
                
                // Запускаем WebAuthenticator
                var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                    new Uri(authUrl),
                    new Uri($"{CallbackScheme}://oauth"));

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
            sb.Append($"&client_id={ClientId}");
            
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
