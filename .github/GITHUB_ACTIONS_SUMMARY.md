# 📦 GitHub Actions для Library - Краткая справка

## ⚡ Быстрые ссылки

| Документ | Описание |
|----------|----------|
| [README.md](README.md) | Обзор и быстрый старт |
| [SETUP_SECRETS.md](SETUP_SECRETS.md) | Настройка секретов GitHub |
| [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) | Полное руководство по развёртыванию |
| [REMOVE_KEYSTORE.md](REMOVE_KEYSTORE.md) | ⚠️ Удаление keystore из репозитория |
| [scripts/README.md](scripts/README.md) | Документация PowerShell скриптов |

## 🎯 Основные задачи

### Первая настройка

```powershell
# 1. Создайте или конвертируйте keystore
.\.github\scripts\convert-keystore.ps1

# 2. Добавьте секреты в GitHub:
#    - ANDROID_KEYSTORE_BASE64
#    - ANDROID_KEY_PASSWORD
#    - ANDROID_STORE_PASSWORD

# 3. Создайте первый релиз
git tag v1.13
git push origin v1.13
```

### Создание релиза

```powershell
# 1. Обновите версию в Library.csproj
# 2. Закоммитьте изменения
git add Library.csproj
git commit -m "Release v1.14"
git push

# 3. Создайте тег
git tag v1.14
git push origin v1.14

# 4. APK будет автоматически создан и опубликован
```

### Ручная сборка

1. Перейдите в **Actions**
2. Выберите **Android Build and Release**
3. Нажмите **Run workflow**
4. Выберите ветку и нажмите **Run workflow**

## 📊 Структура workflows

```
.github/
├── workflows/
│   ├── android-build.yml      # Основной workflow (Debug + Release)
│   └── pr-check.yml           # Проверка PR (только Debug)
├── scripts/
│   ├── create-keystore.ps1    # Создание нового keystore
│   ├── convert-keystore.ps1   # Конвертация в Base64
│   └── README.md              # Документация скриптов
├── ISSUE_TEMPLATE/
│   └── build-issue.md         # Шаблон для проблем сборки
└── [документация]
```

## 🔄 Процесс работы workflow

### Debug сборка (Push/PR)
```
Push/PR → Checkout → Setup .NET → Install MAUI → 
Restore → Build Debug → Upload APK
```

### Release сборка (Tag)
```
Tag Push → Checkout → Setup .NET → Install MAUI → 
Restore → Decode Keystore → Build & Sign → 
Upload APK → Create GitHub Release → Cleanup
```

## 🔐 Безопасность

### ✅ Что делать:
- Хранить keystore в GitHub Secrets (Base64)
- Использовать `.gitignore` для `*.keystore`
- Делать резервные копии keystore
- Использовать менеджер паролей

### ❌ Что НЕ делать:
- Коммитить keystore файлы
- Публиковать пароли в коде
- Делиться keystore с посторонними
- Хранить пароли в открытом виде

## 📋 Чек-лист релиза

Перед каждым релизом проверьте:

- [ ] Версия увеличена в `Library.csproj`
- [ ] `CHANGELOG.md` обновлён
- [ ] Локальная сборка работает
- [ ] Все секреты настроены в GitHub
- [ ] `.gitignore` включает `*.keystore`
- [ ] Нет keystore файлов в репозитории (критично!)
- [ ] Тег создан правильно (формат: `vX.Y.Z`)

## 🆘 Частые проблемы

| Проблема | Решение |
|----------|---------|
| "Keystore not found" | Настройте `ANDROID_KEYSTORE_BASE64` |
| "Wrong password" | Проверьте `ANDROID_KEY_PASSWORD` и `ANDROID_STORE_PASSWORD` |
| "Workflow not running" | Проверьте Actions в Settings |
| "Invalid base64" | Пересоздайте Base64 без переносов строк |
| "Build failed" | Проверьте логи в Actions → Workflow run |

## 📱 Текущая конфигурация

```yaml
Application ID:  ru.rapaner.library
Version:         1.13 (build 13)
Target:          net9.0-android
Min SDK:         21 (Android 5.0)
Keystore alias:  myappalias
Keystore file:   library.keystore
```

## 🔗 Полезные команды

### Локальная сборка Release
```powershell
dotnet publish Library.csproj -c Release -f net9.0-android /p:AndroidPackageFormats=apk
```

### Проверка keystore
```powershell
keytool -list -v -keystore library.keystore
```

### Просмотр APK информации
```powershell
aapt dump badging app.apk | Select-String "version"
```

### Создание тега
```powershell
git tag v1.14
git push origin v1.14
```

### Удаление тега (если ошиблись)
```powershell
# Локально
git tag -d v1.14

# На GitHub
git push origin :refs/tags/v1.14
```

## 📈 Мониторинг

### Где смотреть статус:
1. **GitHub Actions** - общий статус
2. **Releases** - опубликованные APK
3. **Artifacts** - временные сборки (7-90 дней)

### Уведомления:
- GitHub отправляет email при провале workflow
- Можно настроить Slack/Discord интеграцию

## 🎓 Дополнительное обучение

### Темы для изучения:
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [.NET MAUI Android Publishing](https://docs.microsoft.com/en-us/dotnet/maui/android/deployment/)
- [Android App Signing](https://developer.android.com/studio/publish/app-signing)
- [Semantic Versioning](https://semver.org/)

## ⚠️ КРИТИЧНО: Безопасность keystore

**Файл `library.keystore` обнаружен в Git репозитории!**

Это серьёзная проблема безопасности. Немедленно выполните инструкции в [REMOVE_KEYSTORE.md](REMOVE_KEYSTORE.md).

## 💡 Советы

1. **Используйте семантическое версионирование:** `vMAJOR.MINOR.PATCH`
2. **Создавайте теги только для стабильных версий**
3. **Используйте ветки для feature development**
4. **Тестируйте локально перед push тегов**
5. **Храните keystore в нескольких безопасных местах**
6. **Документируйте изменения в CHANGELOG.md**

## 📞 Поддержка

Если возникли проблемы:
1. Проверьте документацию в `.github/`
2. Посмотрите логи в GitHub Actions
3. Создайте Issue используя шаблон
4. Проверьте [GitHub Community](https://github.community/)

---

**Версия документации:** 1.0  
**Последнее обновление:** 2025-10-16  
**Автор:** GitHub Actions Setup Script

