# Скрипт для создания нового Android keystore
# Использование: .\create-keystore.ps1

param(
    [Parameter(Mandatory=$false)]
    [string]$KeystoreName = "library.keystore",
    
    [Parameter(Mandatory=$false)]
    [string]$Alias = "myappalias",
    
    [Parameter(Mandatory=$false)]
    [int]$Validity = 10000
)

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Создание Android Keystore" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Проверка наличия keytool
try {
    $null = Get-Command keytool -ErrorAction Stop
} catch {
    Write-Host "❌ ОШИБКА: keytool не найден!" -ForegroundColor Red
    Write-Host ""
    Write-Host "keytool входит в состав Java JDK." -ForegroundColor Yellow
    Write-Host "Установите Java JDK и убедитесь, что он в PATH." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Скачать JDK можно здесь:" -ForegroundColor Yellow
    Write-Host "  - Oracle JDK: https://www.oracle.com/java/technologies/downloads/" -ForegroundColor Gray
    Write-Host "  - OpenJDK: https://adoptium.net/" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host "✅ Java keytool найден" -ForegroundColor Green
Write-Host ""

# Проверка существования файла
if (Test-Path $KeystoreName) {
    Write-Host "⚠️  ВНИМАНИЕ: Файл '$KeystoreName' уже существует!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Если вы продолжите, старый keystore будет ПЕРЕЗАПИСАН." -ForegroundColor Red
    Write-Host "Это означает, что вы НЕ сможете обновлять приложения," -ForegroundColor Red
    Write-Host "подписанные старым keystore!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Продолжить? (yes/no): " -NoNewline -ForegroundColor Yellow
    $response = Read-Host
    
    if ($response -ne 'yes') {
        Write-Host ""
        Write-Host "❌ Операция отменена" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Совет: Используйте другое имя файла:" -ForegroundColor Cyan
        Write-Host "  .\create-keystore.ps1 -KeystoreName 'library-new.keystore'" -ForegroundColor Gray
        exit 0
    }
    
    Write-Host ""
    Write-Host "⚠️  Создаём резервную копию..." -ForegroundColor Yellow
    $backupName = "$KeystoreName.backup.$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    Copy-Item $KeystoreName $backupName
    Write-Host "✅ Резервная копия: $backupName" -ForegroundColor Green
    Write-Host ""
}

# Информация о параметрах
Write-Host "📋 Параметры keystore:" -ForegroundColor Cyan
Write-Host "   Файл:     $KeystoreName" -ForegroundColor Gray
Write-Host "   Алиас:    $Alias" -ForegroundColor Gray
Write-Host "   Срок:     $Validity дней (~27 лет)" -ForegroundColor Gray
Write-Host ""

# Запрос информации
Write-Host "🔐 Введите информацию для keystore:" -ForegroundColor Cyan
Write-Host ""

Write-Host "Пароль keystore (минимум 6 символов): " -NoNewline -ForegroundColor Yellow
$storePassword = Read-Host -AsSecureString
$storePasswordText = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($storePassword)
)

if ($storePasswordText.Length -lt 6) {
    Write-Host ""
    Write-Host "❌ ОШИБКА: Пароль должен быть не менее 6 символов!" -ForegroundColor Red
    exit 1
}

Write-Host "Пароль ключа (Enter = такой же как у keystore): " -NoNewline -ForegroundColor Yellow
$keyPassword = Read-Host -AsSecureString
$keyPasswordText = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($keyPassword)
)

if ([string]::IsNullOrWhiteSpace($keyPasswordText)) {
    $keyPasswordText = $storePasswordText
    Write-Host "   Используется тот же пароль" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Имя и Фамилия (например, Ivan Petrov): " -NoNewline -ForegroundColor Yellow
$name = Read-Host

Write-Host "Организация (например, My Company): " -NoNewline -ForegroundColor Yellow
$organization = Read-Host

Write-Host "Город (например, Moscow): " -NoNewline -ForegroundColor Yellow
$city = Read-Host

Write-Host "Регион/Область (например, Moscow): " -NoNewline -ForegroundColor Yellow
$state = Read-Host

Write-Host "Код страны (2 буквы, например, RU): " -NoNewline -ForegroundColor Yellow
$country = Read-Host

Write-Host ""
Write-Host "🔄 Создание keystore..." -ForegroundColor Yellow

# Формирование команды keytool
$dname = "CN=$name, OU=$organization, O=$organization, L=$city, S=$state, C=$country"

try {
    # Создание временного скрипта для автоматического ввода паролей
    $tempScript = [System.IO.Path]::GetTempFileName()
    @"
$storePasswordText
$storePasswordText
$keyPasswordText
$keyPasswordText

"@ | Out-File -FilePath $tempScript -Encoding ASCII

    # Выполнение keytool
    $process = Start-Process -FilePath "keytool" -ArgumentList @(
        "-genkeypair",
        "-v",
        "-keystore", $KeystoreName,
        "-alias", $Alias,
        "-keyalg", "RSA",
        "-keysize", "2048",
        "-validity", $Validity,
        "-dname", $dname
    ) -NoNewWindow -Wait -PassThru -RedirectStandardInput $tempScript -RedirectStandardError "keytool-error.log"

    # Очистка временного файла
    Remove-Item $tempScript -Force -ErrorAction SilentlyContinue

    if ($process.ExitCode -eq 0) {
        Write-Host "✅ Keystore успешно создан!" -ForegroundColor Green
    } else {
        Write-Host "❌ ОШИБКА при создании keystore!" -ForegroundColor Red
        if (Test-Path "keytool-error.log") {
            Write-Host ""
            Write-Host "Лог ошибки:" -ForegroundColor Yellow
            Get-Content "keytool-error.log"
            Remove-Item "keytool-error.log" -Force
        }
        exit 1
    }
} catch {
    Write-Host "❌ ОШИБКА: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Информация о созданном keystore" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Вывод информации о keystore
Write-Host "📄 Файл: " -NoNewline
Write-Host "$KeystoreName" -ForegroundColor Green

$fileSize = (Get-Item $KeystoreName).Length
Write-Host "📊 Размер: " -NoNewline
Write-Host "$fileSize bytes" -ForegroundColor Green

Write-Host ""
Write-Host "🔑 Алиас: " -NoNewline
Write-Host "$Alias" -ForegroundColor Green

Write-Host "👤 DN: " -NoNewline
Write-Host "$dname" -ForegroundColor Green

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  ВАЖНАЯ ИНФОРМАЦИЯ - СОХРАНИТЕ ЭТО!" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "📝 Сохраните следующую информацию в безопасном месте:" -ForegroundColor Yellow
Write-Host ""
Write-Host "   Файл keystore:    $KeystoreName" -ForegroundColor White
Write-Host "   Алиас ключа:      $Alias" -ForegroundColor White
Write-Host "   Пароль keystore:  [тот, что вы ввели]" -ForegroundColor White
Write-Host "   Пароль ключа:     [тот, что вы ввели]" -ForegroundColor White
Write-Host ""

Write-Host "⚠️  БЕЗ ЭТОЙ ИНФОРМАЦИИ ВЫ НЕ СМОЖЕТЕ:" -ForegroundColor Red
Write-Host "   - Обновлять опубликованное приложение" -ForegroundColor Red
Write-Host "   - Подписывать новые версии" -ForegroundColor Red
Write-Host "   - Восстановить доступ к приложению в Google Play" -ForegroundColor Red
Write-Host ""

Write-Host "💾 Рекомендации по хранению:" -ForegroundColor Cyan
Write-Host "   ✅ Сделайте резервную копию keystore файла" -ForegroundColor Green
Write-Host "   ✅ Храните пароли в менеджере паролей" -ForegroundColor Green
Write-Host "   ✅ Храните копию в облаке (зашифрованной)" -ForegroundColor Green
Write-Host "   ❌ НЕ коммитьте keystore в Git" -ForegroundColor Red
Write-Host "   ❌ НЕ делитесь паролями" -ForegroundColor Red
Write-Host ""

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Следующие шаги" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "1️⃣  Конвертируйте keystore в Base64 для GitHub:" -ForegroundColor Yellow
Write-Host "   .\.github\scripts\convert-keystore.ps1" -ForegroundColor Gray
Write-Host ""

Write-Host "2️⃣  Настройте GitHub Secrets (см. .github/SETUP_SECRETS.md)" -ForegroundColor Yellow
Write-Host ""

Write-Host "3️⃣  Обновите Library.csproj если алиас отличается:" -ForegroundColor Yellow
Write-Host "   <AndroidSigningKeyAlias>$Alias</AndroidSigningKeyAlias>" -ForegroundColor Gray
Write-Host ""

Write-Host "✨ Готово! Keystore создан успешно!" -ForegroundColor Cyan
Write-Host ""

