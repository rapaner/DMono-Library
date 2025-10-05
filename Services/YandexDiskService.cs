using YandexDisk.Client;
using YandexDisk.Client.Http;
using YandexDisk.Client.Protocol;

namespace Library.Services
{
    /// <summary>
    /// Сервис для работы с Яндекс Диском
    /// </summary>
    public class YandexDiskService
    {
        private IDiskApi? _diskApi;
        private string? _oauthToken;

        /// <summary>
        /// Проверяет, авторизован ли пользователь
        /// </summary>
        public bool IsAuthorized => !string.IsNullOrEmpty(_oauthToken) && _diskApi != null;

        /// <summary>
        /// Установить OAuth токен для авторизации
        /// </summary>
        /// <param name="oauthToken">OAuth токен Яндекс Диска</param>
        public void SetOAuthToken(string oauthToken)
        {
            _oauthToken = oauthToken;
            _diskApi = new DiskHttpApi(oauthToken);
        }

        /// <summary>
        /// Получить информацию о диске (свободное место, занятое место и т.д.)
        /// </summary>
        /// <returns>Информация о диске</returns>
        /// <exception cref="InvalidOperationException">Если пользователь не авторизован</exception>
        public async Task<Disk> GetDiskInfoAsync()
        {
            EnsureAuthorized();
            return await _diskApi!.MetaInfo.GetDiskInfoAsync();
        }

        /// <summary>
        /// Загрузить файл базы данных на Яндекс Диск
        /// </summary>
        /// <param name="localFilePath">Путь к локальному файлу</param>
        /// <param name="remotePath">Путь на Яндекс Диске (например, "/backup/library.db")</param>
        /// <param name="overwrite">Перезаписать файл, если он уже существует</param>
        /// <returns>True, если загрузка прошла успешно</returns>
        /// <exception cref="InvalidOperationException">Если пользователь не авторизован</exception>
        public async Task<bool> UploadFileAsync(string localFilePath, string remotePath, bool overwrite = true)
        {
            try
            {
                EnsureAuthorized();

                if (!File.Exists(localFilePath))
                {
                    throw new FileNotFoundException($"Файл не найден: {localFilePath}");
                }

                // Создаем директорию на Яндекс Диске, если её нет
                var directory = Path.GetDirectoryName(remotePath)?.Replace("\\", "/");
                if (!string.IsNullOrEmpty(directory) && directory != "/")
                {
                    await CreateDirectoryAsync(directory);
                }

                // Получаем ссылку для загрузки
                var link = await _diskApi!.Files.GetUploadLinkAsync(remotePath, overwrite: overwrite);

                // Загружаем файл
                using var fileStream = File.OpenRead(localFilePath);
                await _diskApi.Files.UploadAsync(link, fileStream);

                return true;
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке файла на Яндекс Диск: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Скачать файл с Яндекс Диска
        /// </summary>
        /// <param name="remotePath">Путь к файлу на Яндекс Диске</param>
        /// <param name="localFilePath">Путь для сохранения локального файла</param>
        /// <returns>True, если скачивание прошло успешно</returns>
        /// <exception cref="InvalidOperationException">Если пользователь не авторизован</exception>
        public async Task<bool> DownloadFileAsync(string remotePath, string localFilePath)
        {
            try
            {
                EnsureAuthorized();

                // Получаем ссылку для скачивания
                var link = await _diskApi!.Files.GetDownloadLinkAsync(remotePath);

                // Скачиваем файл
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync(link.Href);
                response.EnsureSuccessStatusCode();

                // Создаем директорию, если её нет
                var directory = Path.GetDirectoryName(localFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Сохраняем файл
                using var fileStream = File.Create(localFilePath);
                await response.Content.CopyToAsync(fileStream);

                return true;
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                System.Diagnostics.Debug.WriteLine($"Ошибка при скачивании файла с Яндекс Диска: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Проверить, существует ли файл на Яндекс Диске
        /// </summary>
        /// <param name="remotePath">Путь к файлу на Яндекс Диске</param>
        /// <returns>True, если файл существует</returns>
        /// <exception cref="InvalidOperationException">Если пользователь не авторизован</exception>
        public async Task<bool> FileExistsAsync(string remotePath)
        {
            try
            {
                EnsureAuthorized();
                var resource = await _diskApi!.MetaInfo.GetInfoAsync(new ResourceRequest { Path = remotePath });
                return resource != null && resource.Type == ResourceType.File;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Получить список файлов в директории на Яндекс Диске
        /// </summary>
        /// <param name="remotePath">Путь к директории на Яндекс Диске</param>
        /// <returns>Список ресурсов (файлов и папок)</returns>
        /// <exception cref="InvalidOperationException">Если пользователь не авторизован</exception>
        public async Task<YandexDisk.Client.Protocol.Resource> GetFilesAsync(string remotePath)
        {
            EnsureAuthorized();
            return await _diskApi!.MetaInfo.GetInfoAsync(new ResourceRequest { Path = remotePath });
        }

        /// <summary>
        /// Создать директорию на Яндекс Диске
        /// </summary>
        /// <param name="remotePath">Путь к директории</param>
        /// <returns>True, если директория создана или уже существует</returns>
        /// <exception cref="InvalidOperationException">Если пользователь не авторизован</exception>
        public async Task<bool> CreateDirectoryAsync(string remotePath)
        {
            try
            {
                EnsureAuthorized();
                
                // Проверяем, существует ли директория
                try
                {
                    var resource = await _diskApi!.MetaInfo.GetInfoAsync(new ResourceRequest { Path = remotePath });
                    if (resource != null && resource.Type == ResourceType.Dir)
                    {
                        return true; // Директория уже существует
                    }
                }
                catch
                {
                    // Директория не существует, создаем её
                }

                // Создаем директорию
                await _diskApi!.Commands.CreateDictionaryAsync(remotePath);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при создании директории на Яндекс Диске: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Удалить файл с Яндекс Диска
        /// </summary>
        /// <param name="remotePath">Путь к файлу на Яндекс Диске</param>
        /// <param name="permanently">Удалить навсегда (true) или переместить в корзину (false)</param>
        /// <returns>True, если удаление прошло успешно</returns>
        /// <exception cref="InvalidOperationException">Если пользователь не авторизован</exception>
        public async Task<bool> DeleteFileAsync(string remotePath, bool permanently = false)
        {
            try
            {
                EnsureAuthorized();
                await _diskApi!.Commands.DeleteAsync(new DeleteFileRequest { Path = remotePath, Permanently = permanently });
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при удалении файла с Яндекс Диска: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Загрузить резервную копию базы данных на Яндекс Диск
        /// </summary>
        /// <param name="dbPath">Путь к файлу базы данных</param>
        /// <returns>True, если резервное копирование прошло успешно</returns>
        public async Task<bool> BackupDatabaseAsync(string dbPath)
        {
            try
            {
                var remotePath = $"/Library_Backups/library_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                return await UploadFileAsync(dbPath, remotePath, overwrite: false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при резервном копировании базы данных: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Восстановить базу данных из резервной копии на Яндекс Диске
        /// </summary>
        /// <param name="remotePath">Путь к резервной копии на Яндекс Диске</param>
        /// <param name="localDbPath">Путь для сохранения локальной базы данных</param>
        /// <returns>True, если восстановление прошло успешно</returns>
        public async Task<bool> RestoreDatabaseAsync(string remotePath, string localDbPath)
        {
            try
            {
                // Создаем резервную копию текущей базы данных перед восстановлением
                if (File.Exists(localDbPath))
                {
                    var backupPath = $"{localDbPath}.backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(localDbPath, backupPath, overwrite: true);
                }

                return await DownloadFileAsync(remotePath, localDbPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при восстановлении базы данных: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Получить список резервных копий базы данных на Яндекс Диске
        /// </summary>
        /// <returns>Список файлов резервных копий</returns>
        public async Task<List<YandexDisk.Client.Protocol.Resource>> GetBackupListAsync()
        {
            try
            {
                EnsureAuthorized();
                var resource = await _diskApi!.MetaInfo.GetInfoAsync(new ResourceRequest { Path = "/Library_Backups" });
                
                if (resource?.Embedded?.Items != null)
                {
                    return resource.Embedded.Items
                        .Where(item => item.Type == ResourceType.File && item.Name.EndsWith(".db"))
                        .OrderByDescending(item => item.Created)
                        .ToList();
                }
                
                return new List<YandexDisk.Client.Protocol.Resource>();
            }
            catch
            {
                return new List<YandexDisk.Client.Protocol.Resource>();
            }
        }

        /// <summary>
        /// Проверяет, что пользователь авторизован
        /// </summary>
        /// <exception cref="InvalidOperationException">Если пользователь не авторизован</exception>
        private void EnsureAuthorized()
        {
            if (!IsAuthorized)
            {
                throw new InvalidOperationException("Необходима авторизация в Яндекс Диске. Установите OAuth токен с помощью SetOAuthToken()");
            }
        }
    }
}
