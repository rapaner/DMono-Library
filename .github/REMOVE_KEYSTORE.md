# ⚠️ КРИТИЧНО: Удаление keystore из Git репозитория

## Проблема

Файл `library.keystore` был найден в Git истории. Это серьёзная проблема безопасности, так как любой имеющий доступ к репозиторию может получить ваш keystore.

## Решение

### Вариант 1: Удалить из истории и создать новый keystore (РЕКОМЕНДУЕТСЯ)

Если репозиторий публичный или доступ к нему имеют посторонние:

1. **Создайте новый keystore:**
   ```powershell
   keytool -genkeypair -v -keystore library.keystore -alias dmonolibrary -keyalg RSA -keysize 2048 -validity 10000
   ```

2. **Удалите старый keystore из Git истории:**
   ```powershell
   git filter-branch --force --index-filter "git rm --cached --ignore-unmatch library.keystore" --prune-empty --tag-name-filter cat -- --all
   ```

   Или используйте BFG Repo-Cleaner (быстрее):
   ```powershell
   # Скачайте BFG с https://rtyley.github.io/bfg-repo-cleaner/
   java -jar bfg.jar --delete-files library.keystore
   git reflog expire --expire=now --all
   git gc --prune=now --aggressive
   ```

3. **Force push (ВНИМАНИЕ: опасная операция):**
   ```powershell
   git push origin --force --all
   git push origin --force --tags
   ```

4. **Сообщите всем участникам:**
   Всем, кто клонировал репозиторий, нужно удалить локальные копии и клонировать заново.

### Вариант 2: Только удалить из текущего состояния (если репозиторий приватный)

Если репозиторий приватный и доступ строго контролируется:

1. **Удалите файл из Git:**
   ```powershell
   git rm --cached library.keystore
   git commit -m "Remove keystore from repository"
   git push
   ```

2. **Keystore останется в истории**, но не будет в новых коммитах.

## После удаления

1. **Убедитесь, что .gitignore обновлен** (уже сделано в текущем коммите):
   ```
   # Android keystore files
   *.keystore
   *.jks
   keystore_base64.txt
   ```

2. **Настройте GitHub Secrets** согласно `.github/SETUP_SECRETS.md`

3. **Храните keystore в безопасном месте:**
   - Сделайте резервную копию в надёжном месте
   - Не храните в облачных сервисах общего доступа
   - Используйте менеджер паролей для хранения паролей

## Проверка

Убедитесь, что keystore больше не в репозитории:

```powershell
git ls-files | Select-String -Pattern "keystore"
```

Результат должен быть пустым.

## Важные примечания

⚠️ **ВНИМАНИЕ:**
- После создания нового keystore все будущие обновления приложения должны быть подписаны НОВЫМ keystore
- Если вы уже опубликовали приложение в Google Play с СТАРЫМ keystore, вам понадобится старый keystore для обновлений
- В этом случае используйте только Вариант 2 и обеспечьте строгий контроль доступа к репозиторию

## Google Play App Signing

Если используете Google Play App Signing:
- Google хранит production keystore
- Вы используете upload keystore для загрузки
- В этом случае создание нового upload keystore не проблема
- Обновите upload certificate в Google Play Console

