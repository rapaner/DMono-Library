# Отпечатки сертификата Android приложения

## Debug Keystore

**Расположение**: `%USERPROFILE%\.android\debug.keystore`

**Alias**: androiddebugkey

**Пароли**:
- Store password: android
- Key password: android

## Отпечатки сертификата (Certificate Fingerprints)

### SHA-1
```
BD:45:CF:AE:9B:0B:5F:A5:D8:DE:70:83:43:52:C1:AB:22:F0:C7:AF
```

### SHA-256
```
9A:33:21:F8:E3:60:30:25:4A:B4:37:09:80:C8:27:27:E1:F1:7E:A0:9C:56:8B:C9:5A:4A:2F:C3:CB:6A:2B:5F
```

## Информация о сертификате

- **Owner**: CN=Android Debug, O=Android, C=US
- **Issuer**: CN=Android Debug, O=Android, C=US
- **Serial number**: f7ce2b2f79dd6129
- **Valid from**: Mon Oct 06 21:11:22 MSK 2025
- **Valid until**: Fri Feb 21 21:11:22 MSK 2053
- **Key Algorithm**: 2048-bit RSA key
- **Signature Algorithm**: SHA256withRSA

## Использование

### Для Yandex OAuth

При регистрации приложения в Яндекс OAuth (https://oauth.yandex.ru/) вам потребуется указать:

1. **Package Name**: `ru.rapaner.library`
2. **SHA-1 отпечаток**: `BD:45:CF:AE:9B:0B:5F:A5:D8:DE:70:83:43:52:C1:AB:22:F0:C7:AF`

### Для Firebase/Google Services

Используйте SHA-1 отпечаток при добавлении приложения в Firebase Console.

### Для production релиза

⚠️ **ВАЖНО**: Этот сертификат используется только для отладки (debug). 

Для релизной версии приложения необходимо:
1. Создать отдельный release keystore
2. Получить отпечатки для release сертификата
3. Обновить настройки в консолях разработчика (Yandex OAuth, Firebase и т.д.)

## Как получить отпечатки снова

Выполните команду:
```bash
keytool -list -v -keystore "%USERPROFILE%\.android\debug.keystore" -alias androiddebugkey -storepass android -keypass android
```


