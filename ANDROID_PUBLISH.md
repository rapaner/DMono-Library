# Публикация на Android

Tools -> Command Line -> Developer PowerShell

Команда - `dotnet publish -f net9.0-android -c Release /p:AndroidSigningKeyPass=Atljh246 /p:AndroidSigningStorePass=Atljh246 /p:YandexOAuth__ClientId=92dbf014613440249d8c6fe833f1e368 /p:YandexOAuth__CallbackScheme=ru.rapaner.library`

Установочный файл - `bin\Release\net9.0-android\publish\ru.rapaner.library-Signed.apk`