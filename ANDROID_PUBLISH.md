# Публикация на Android

Tools -> Command Line -> Developer PowerShell

Команда - `dotnet publish -f net9.0-android -c Release /p:AndroidSigningKeyPass=ENV /p:AndroidSigningStorePass=ENV /p:YandexOAuth__ClientId=ENV`

Установочный файл - `bin\Release\net9.0-android\publish\ru.rapaner.library-Signed.apk`