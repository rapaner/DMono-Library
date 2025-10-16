# Настройка GitHub Secrets для Android Build Pipeline

## Необходимые секреты

Для подписи APK в GitHub Actions необходимо настроить следующие секреты:

### 1. ANDROID_KEYSTORE_BASE64

Конвертируйте ваш keystore файл в base64:

**В PowerShell (в папке проекта):**
```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("library.keystore"))
```

Или сохраните в файл:
```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("library.keystore")) | Out-File -FilePath "keystore_base64.txt" -Encoding ASCII
```

### 2. ANDROID_KEY_PASSWORD

Пароль для ключа (key password) из вашего keystore.

### 3. ANDROID_STORE_PASSWORD

Пароль для keystore (store password).

## Как добавить секреты в GitHub

1. Перейдите в ваш репозиторий на GitHub
2. Откройте **Settings** → **Secrets and variables** → **Actions**
3. Нажмите **New repository secret**
4. Добавьте каждый из трёх секретов:

   - **Name:** `ANDROID_KEYSTORE_BASE64`
     **Secret:** _вставьте base64 строку из шага 1_

   - **Name:** `ANDROID_KEY_PASSWORD`
     **Secret:** _ваш пароль ключа_

   - **Name:** `ANDROID_STORE_PASSWORD`
     **Secret:** _ваш пароль keystore_

## Проверка секретов

После добавления секретов, вы должны увидеть три записи в списке секретов:
- ✅ ANDROID_KEYSTORE_BASE64
- ✅ ANDROID_KEY_PASSWORD
- ✅ ANDROID_STORE_PASSWORD

## Использование pipeline

### Автоматическая сборка Debug APK
При push в ветки `main` или `github_pipeline`:
```bash
git push origin main
```

### Создание Release с подписанным APK
Создайте и запушьте тег:
```bash
git tag v1.13
git push origin v1.13
```

### Ручной запуск сборки
Перейдите в **Actions** → **Android Build and Release** → **Run workflow**

## Где найти готовый APK

### Для обычных коммитов (Debug):
1. Перейдите в **Actions**
2. Выберите нужный workflow run
3. Скачайте артефакт **android-debug-apk**

### Для релизов (Signed):
1. **Артефакты:** Actions → workflow run → **android-release-signed-apk**
2. **Релизы:** Перейдите в **Releases** → найдите релиз с вашим тегом

## Безопасность

⚠️ **ВАЖНО:**
- Никогда не коммитьте keystore файлы в репозиторий
- Не публикуйте пароли в коде
- Используйте только GitHub Secrets для хранения чувствительных данных
- Файл `library.keystore` должен быть в `.gitignore`

## Текущая конфигурация

- **Keystore файл:** `library.keystore`
- **Key alias:** `myappalias`
- **Application ID:** `ru.rapaner.library`
- **Версия:** 1.13 (build 13)

## Устранение проблем

### "Keystore not found"
Убедитесь, что секрет `ANDROID_KEYSTORE_BASE64` правильно настроен.

### "Invalid keystore format"
Проверьте, что base64 строка скопирована полностью без переносов строк.

### "Wrong password"
Проверьте секреты `ANDROID_KEY_PASSWORD` и `ANDROID_STORE_PASSWORD`.

### APK не подписан
Убедитесь, что все три секрета добавлены и workflow запущен для тега или вручную.

