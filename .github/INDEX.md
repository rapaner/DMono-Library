# 📚 GitHub Actions Documentation Index

Полная документация по CI/CD pipeline для Library MAUI приложения.

## 🚀 Быстрый старт (для новых пользователей)

**Первая настройка (5-10 минут):**

1. 📖 Прочитайте [README.md](README.md)
2. 🔐 Следуйте [SETUP_SECRETS.md](SETUP_SECRETS.md)
3. 🎯 Создайте первый релиз
4. ✅ Готово!

**Для опытных пользователей:**
- [GITHUB_ACTIONS_SUMMARY.md](GITHUB_ACTIONS_SUMMARY.md) - краткая справка

## 📖 Документация

### Основные руководства

| Документ | Уровень | Описание | Время |
|----------|---------|----------|-------|
| [README.md](README.md) | Начальный | Обзор и быстрый старт | 5 мин |
| [SETUP_SECRETS.md](SETUP_SECRETS.md) | Начальный | Настройка GitHub Secrets | 10 мин |
| [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) | Средний | Полное руководство по развёртыванию | 20 мин |
| [GITHUB_ACTIONS_SUMMARY.md](GITHUB_ACTIONS_SUMMARY.md) | Любой | Краткая справка | 3 мин |
| [PIPELINE_OVERVIEW.md](PIPELINE_OVERVIEW.md) | Средний | Визуальный обзор pipeline | 15 мин |

### Специальные темы

| Документ | Описание | Важность |
|----------|----------|----------|
| [REMOVE_KEYSTORE.md](REMOVE_KEYSTORE.md) | ⚠️ Удаление keystore из Git | 🔴 КРИТИЧНО |
| [scripts/README.md](scripts/README.md) | PowerShell скрипты | 🟡 Полезно |
| [ISSUE_TEMPLATE/build-issue.md](ISSUE_TEMPLATE/build-issue.md) | Шаблон для багов | 🟢 Опционально |

## 🛠️ Инструменты

### PowerShell Скрипты

| Скрипт | Назначение | Использование |
|--------|------------|---------------|
| [create-keystore.ps1](scripts/create-keystore.ps1) | Создать новый keystore | `.\create-keystore.ps1` |
| [convert-keystore.ps1](scripts/convert-keystore.ps1) | Конвертировать в Base64 | `.\convert-keystore.ps1` |

Подробнее: [scripts/README.md](scripts/README.md)

## 🔄 Workflows

| Workflow | Триггер | Назначение | Результат |
|----------|---------|------------|-----------|
| [android-build.yml](workflows/android-build.yml) | Push, Tag, Manual | Основная сборка | Debug/Release APK |
| [pr-check.yml](workflows/pr-check.yml) | Pull Request | Проверка PR | Build Status |

## 📋 Шаблоны

| Шаблон | Назначение |
|--------|------------|
| [build-issue.md](ISSUE_TEMPLATE/build-issue.md) | Сообщить о проблеме сборки |

## 🎯 Сценарии использования

### Сценарий 1: Первая настройка

```
1. [README.md](README.md)
   → Общий обзор
   
2. [SETUP_SECRETS.md](SETUP_SECRETS.md)
   → Настройка секретов
   
3. [scripts/convert-keystore.ps1](scripts/convert-keystore.ps1)
   → Конвертация keystore
   
4. Создать первый релиз
```

### Сценарий 2: Создание релиза

```
1. [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md)
   → Раздел "Процесс релиза"
   
2. Обновить версию в Library.csproj

3. Создать и запушить тег
```

### Сценарий 3: Проблемы с безопасностью

```
1. [REMOVE_KEYSTORE.md](REMOVE_KEYSTORE.md)
   → Удаление keystore из репозитория
   
2. [scripts/create-keystore.ps1](scripts/create-keystore.ps1)
   → Создать новый keystore
   
3. [SETUP_SECRETS.md](SETUP_SECRETS.md)
   → Обновить секреты
```

### Сценарий 4: Устранение неполадок

```
1. [GITHUB_ACTIONS_SUMMARY.md](GITHUB_ACTIONS_SUMMARY.md)
   → Раздел "Частые проблемы"
   
2. [PIPELINE_OVERVIEW.md](PIPELINE_OVERVIEW.md)
   → Раздел "Troubleshooting Flow"
   
3. [ISSUE_TEMPLATE/build-issue.md](ISSUE_TEMPLATE/build-issue.md)
   → Создать issue
```

## 🎓 Путь обучения

### Уровень 1: Новичок (0-1 неделя)

1. ✅ [README.md](README.md) - Понять основы
2. ✅ [SETUP_SECRETS.md](SETUP_SECRETS.md) - Настроить окружение
3. ✅ [GITHUB_ACTIONS_SUMMARY.md](GITHUB_ACTIONS_SUMMARY.md) - Запомнить команды
4. ✅ Создать первый релиз
5. ✅ Скачать и установить APK

**Цель:** Успешно создать релиз через GitHub Actions

### Уровень 2: Продвинутый (1-2 недели)

1. ✅ [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) - Полный процесс
2. ✅ [PIPELINE_OVERVIEW.md](PIPELINE_OVERVIEW.md) - Понять архитектуру
3. ✅ [scripts/README.md](scripts/README.md) - Использовать скрипты
4. ✅ Настроить автоматизацию
5. ✅ Оптимизировать процесс

**Цель:** Полностью автоматизированный процесс релиза

### Уровень 3: Эксперт (2+ недели)

1. ✅ Модифицировать workflows
2. ✅ Добавить собственные этапы
3. ✅ Интегрировать с другими сервисами
4. ✅ Оптимизировать время сборки
5. ✅ Настроить мониторинг

**Цель:** Кастомизированный CI/CD под ваши нужды

## 📊 Карта документации

```
GitHub Actions Documentation
│
├── 🚀 Начало работы
│   ├── README.md (Обзор)
│   ├── SETUP_SECRETS.md (Первая настройка)
│   └── GITHUB_ACTIONS_SUMMARY.md (Шпаргалка)
│
├── 📚 Подробные руководства
│   ├── DEPLOYMENT_GUIDE.md (Развёртывание)
│   ├── PIPELINE_OVERVIEW.md (Архитектура)
│   └── REMOVE_KEYSTORE.md (Безопасность)
│
├── 🛠️ Инструменты
│   └── scripts/
│       ├── README.md
│       ├── create-keystore.ps1
│       └── convert-keystore.ps1
│
├── 🔄 Workflows
│   └── workflows/
│       ├── android-build.yml
│       └── pr-check.yml
│
└── 📋 Шаблоны
    └── ISSUE_TEMPLATE/
        └── build-issue.md
```

## 🔍 Поиск по темам

### По функциональности

**Создание keystore:**
- [scripts/create-keystore.ps1](scripts/create-keystore.ps1)
- [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) → "Управление ключами"

**Конвертация в Base64:**
- [scripts/convert-keystore.ps1](scripts/convert-keystore.ps1)
- [SETUP_SECRETS.md](SETUP_SECRETS.md)

**Создание релиза:**
- [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) → "Процесс релиза"
- [GITHUB_ACTIONS_SUMMARY.md](GITHUB_ACTIONS_SUMMARY.md) → "Создание релиза"

**Безопасность:**
- [REMOVE_KEYSTORE.md](REMOVE_KEYSTORE.md)
- [SETUP_SECRETS.md](SETUP_SECRETS.md)
- [GITHUB_ACTIONS_SUMMARY.md](GITHUB_ACTIONS_SUMMARY.md) → "Безопасность"

**Устранение проблем:**
- [GITHUB_ACTIONS_SUMMARY.md](GITHUB_ACTIONS_SUMMARY.md) → "Частые проблемы"
- [PIPELINE_OVERVIEW.md](PIPELINE_OVERVIEW.md) → "Troubleshooting"
- [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) → "Полезные команды"

### По типу документа

**Руководства (How-to):**
- [SETUP_SECRETS.md](SETUP_SECRETS.md)
- [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md)
- [REMOVE_KEYSTORE.md](REMOVE_KEYSTORE.md)

**Справочники (Reference):**
- [GITHUB_ACTIONS_SUMMARY.md](GITHUB_ACTIONS_SUMMARY.md)
- [scripts/README.md](scripts/README.md)

**Обзоры (Overview):**
- [README.md](README.md)
- [PIPELINE_OVERVIEW.md](PIPELINE_OVERVIEW.md)
- [INDEX.md](INDEX.md) (этот файл)

**Технические (Technical):**
- [workflows/android-build.yml](workflows/android-build.yml)
- [workflows/pr-check.yml](workflows/pr-check.yml)

## 📌 Часто используемые страницы

**Top 5 для ежедневного использования:**

1. [GITHUB_ACTIONS_SUMMARY.md](GITHUB_ACTIONS_SUMMARY.md) - Быстрые команды
2. [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) - Процесс релиза
3. [workflows/android-build.yml](workflows/android-build.yml) - Главный workflow
4. [scripts/convert-keystore.ps1](scripts/convert-keystore.ps1) - Утилита
5. [SETUP_SECRETS.md](SETUP_SECRETS.md) - Настройка секретов

## ⚡ Быстрые ссылки

### Команды

**Создать релиз:**
```bash
git tag v1.14 && git push origin v1.14
```
→ Подробнее: [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md)

**Конвертировать keystore:**
```powershell
.\.github\scripts\convert-keystore.ps1
```
→ Подробнее: [scripts/README.md](scripts/README.md)

**Локальная сборка:**
```bash
dotnet publish Library.csproj -c Release -f net9.0-android
```
→ Подробнее: [GITHUB_ACTIONS_SUMMARY.md](GITHUB_ACTIONS_SUMMARY.md)

### Внешние ресурсы

- [GitHub Actions Docs](https://docs.github.com/en/actions)
- [.NET MAUI Docs](https://docs.microsoft.com/en-us/dotnet/maui/)
- [Android Publishing](https://developer.android.com/studio/publish)

## ⚠️ Критические предупреждения

### 🔴 БЕЗОПАСНОСТЬ

**Файл `library.keystore` обнаружен в репозитории!**

Немедленно прочитайте: [REMOVE_KEYSTORE.md](REMOVE_KEYSTORE.md)

### 🟡 ВАЖНО

- Всегда делайте резервную копию keystore
- Не теряйте пароли (их невозможно восстановить)
- Проверяйте `.gitignore` перед коммитом

## 📞 Поддержка

1. Проверьте соответствующий раздел документации
2. Используйте поиск по этому индексу
3. Проверьте [GITHUB_ACTIONS_SUMMARY.md](GITHUB_ACTIONS_SUMMARY.md) → "Частые проблемы"
4. Создайте Issue: [ISSUE_TEMPLATE/build-issue.md](ISSUE_TEMPLATE/build-issue.md)

## 📝 Обновления документации

**Последнее обновление:** 2025-10-16  
**Версия:** 1.0.0  
**Автор:** GitHub Actions Setup

---

💡 **Совет:** Добавьте эту страницу в закладки для быстрого доступа к документации!

