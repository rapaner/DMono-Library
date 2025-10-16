# Скрипт для конвертации keystore в base64 для GitHub Secrets
# Использование: .\convert-keystore.ps1 -KeystorePath "library.keystore"

param(
    [Parameter(Mandatory=$false)]
    [string]$KeystorePath = "library.keystore",
    
    [Parameter(Mandatory=$false)]
    [string]$OutputFile = "keystore_base64.txt"
)

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Конвертация Keystore в Base64 для GitHub" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Проверка существования файла
if (-not (Test-Path $KeystorePath)) {
    Write-Host "❌ ОШИБКА: Файл '$KeystorePath' не найден!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Доступные .keystore файлы в текущей директории:" -ForegroundColor Yellow
    Get-ChildItem -Filter "*.keystore" | ForEach-Object {
        Write-Host "  - $($_.Name)" -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "Используйте: .\convert-keystore.ps1 -KeystorePath 'ваш_файл.keystore'" -ForegroundColor Yellow
    exit 1
}

Write-Host "📁 Файл keystore: $KeystorePath" -ForegroundColor Green

# Получение размера файла
$fileSize = (Get-Item $KeystorePath).Length
Write-Host "📊 Размер файла: $fileSize bytes" -ForegroundColor Gray
Write-Host ""

# Конвертация в base64
Write-Host "🔄 Конвертация в Base64..." -ForegroundColor Yellow
try {
    $base64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($KeystorePath))
    Write-Host "✅ Конвертация успешна!" -ForegroundColor Green
} catch {
    Write-Host "❌ ОШИБКА при конвертации: $_" -ForegroundColor Red
    exit 1
}

# Сохранение в файл
Write-Host ""
Write-Host "💾 Сохранение в файл: $OutputFile" -ForegroundColor Yellow
try {
    $base64 | Out-File -FilePath $OutputFile -Encoding ASCII -NoNewline
    Write-Host "✅ Файл сохранён!" -ForegroundColor Green
} catch {
    Write-Host "❌ ОШИБКА при сохранении: $_" -ForegroundColor Red
    exit 1
}

# Информация о результате
Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Результат" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📄 Base64 строка сохранена в: " -NoNewline
Write-Host "$OutputFile" -ForegroundColor Green
Write-Host "📏 Длина Base64 строки: " -NoNewline
Write-Host "$($base64.Length) символов" -ForegroundColor Green
Write-Host ""

# Вывод первых и последних символов для проверки
$previewLength = 50
if ($base64.Length -gt $previewLength * 2) {
    Write-Host "👀 Предпросмотр (первые и последние $previewLength символов):" -ForegroundColor Cyan
    Write-Host "   Начало: " -NoNewline -ForegroundColor Gray
    Write-Host $base64.Substring(0, $previewLength) -ForegroundColor White
    Write-Host "   ...     " -ForegroundColor Gray
    Write-Host "   Конец:  " -NoNewline -ForegroundColor Gray
    Write-Host $base64.Substring($base64.Length - $previewLength) -ForegroundColor White
}

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "  Следующие шаги" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1️⃣  Откройте файл '$OutputFile' и скопируйте содержимое" -ForegroundColor Yellow
Write-Host "2️⃣  Перейдите в GitHub: Settings → Secrets → Actions" -ForegroundColor Yellow
Write-Host "3️⃣  Создайте новый секрет 'ANDROID_KEYSTORE_BASE64'" -ForegroundColor Yellow
Write-Host "4️⃣  Вставьте скопированное содержимое как значение секрета" -ForegroundColor Yellow
Write-Host ""

# Предложение скопировать в буфер обмена
Write-Host "💡 Совет: Хотите скопировать в буфер обмена? (Y/N): " -NoNewline -ForegroundColor Cyan
$response = Read-Host

if ($response -eq 'Y' -or $response -eq 'y') {
    try {
        $base64 | Set-Clipboard
        Write-Host "✅ Base64 строка скопирована в буфер обмена!" -ForegroundColor Green
        Write-Host "   Можете сразу вставлять в GitHub Secrets" -ForegroundColor Gray
    } catch {
        Write-Host "⚠️  Не удалось скопировать в буфер обмена" -ForegroundColor Yellow
        Write-Host "   Откройте файл '$OutputFile' вручную" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "⚠️  ВАЖНО: Не коммитьте файл '$OutputFile' в Git!" -ForegroundColor Red
Write-Host "   Он содержит чувствительные данные!" -ForegroundColor Red
Write-Host ""

# Проверка .gitignore
$gitignorePath = ".gitignore"
if (Test-Path $gitignorePath) {
    $gitignoreContent = Get-Content $gitignorePath -Raw
    if ($gitignoreContent -notmatch "keystore_base64\.txt") {
        Write-Host "⚠️  Файл '$OutputFile' НЕ найден в .gitignore!" -ForegroundColor Yellow
        Write-Host "   Убедитесь, что он добавлен!" -ForegroundColor Yellow
    } else {
        Write-Host "✅ Файл защищён .gitignore" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "✨ Готово! Удачи с настройкой GitHub Actions!" -ForegroundColor Cyan
Write-Host ""

