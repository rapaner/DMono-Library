# GitHub Actions для Android Build

## 🚀 Быстрый старт

### 1. Настройка секретов (обязательно!)

Следуйте инструкциям в [SETUP_SECRETS.md](SETUP_SECRETS.md) для настройки:
- `ANDROID_KEYSTORE_BASE64`
- `ANDROID_KEY_PASSWORD`
- `ANDROID_STORE_PASSWORD`

### 2. Создание релиза

```bash
# Обновите версию в Library.csproj если нужно
# ApplicationDisplayVersion и ApplicationVersion

# Создайте тег
git tag v1.13
git push origin v1.13
```

### 3. Получение APK

- **Автоматический релиз:** GitHub Releases → последний релиз
- **Артефакты:** Actions → выберите workflow run → скачайте артефакт

## 📋 Workflow триггеры

| Событие | Тип сборки | Результат |
|---------|-----------|-----------|
| Push в main/github_pipeline | Debug | APK артефакт (7 дней) |
| Push тега `v*` | Release (signed) | GitHub Release + артефакт (90 дней) |
| Pull Request | Debug | APK артефакт (7 дней) |
| Ручной запуск | Release (signed) | APK артефакт (90 дней) |

## ⚠️ Безопасность

**ВАЖНО:** Файл `library.keystore` был обнаружен в репозитории!

Следуйте инструкциям в [REMOVE_KEYSTORE.md](REMOVE_KEYSTORE.md) для устранения этой проблемы безопасности.

## 📁 Структура

```
.github/
├── workflows/
│   └── android-build.yml       # Основной workflow
├── README.md                   # Это файл
├── SETUP_SECRETS.md           # Инструкции по настройке секретов
└── REMOVE_KEYSTORE.md         # Инструкции по удалению keystore из Git
```

## 🔧 Текущая конфигурация

- **Платформа:** .NET MAUI 9.0
- **Target:** Android (net9.0-android)
- **Application ID:** ru.rapaner.library
- **Текущая версия:** 1.13 (build 13)
- **Keystore alias:** myappalias
- **Min Android SDK:** 21

## 📝 Обновление версии

Отредактируйте `Library.csproj`:

```xml
<ApplicationDisplayVersion>1.14</ApplicationDisplayVersion>
<ApplicationVersion>14</ApplicationVersion>
```

Затем создайте новый тег:

```bash
git add Library.csproj
git commit -m "Bump version to 1.14"
git push
git tag v1.14
git push origin v1.14
```

## 🐛 Устранение проблем

### Workflow не запускается
- Проверьте, что файл `.github/workflows/android-build.yml` существует
- Убедитесь, что Actions включены в Settings → Actions

### Ошибка "Keystore not found"
- Настройте секрет `ANDROID_KEYSTORE_BASE64`
- См. [SETUP_SECRETS.md](SETUP_SECRETS.md)

### Ошибка подписи
- Проверьте все три секрета (keystore, key password, store password)
- Убедитесь, что пароли верны

### APK не создаётся
- Проверьте логи workflow в Actions
- Убедитесь, что все зависимости установлены

## 📚 Дополнительные ресурсы

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [.NET MAUI Documentation](https://docs.microsoft.com/en-us/dotnet/maui/)
- [Android App Signing](https://developer.android.com/studio/publish/app-signing)

