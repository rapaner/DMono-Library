# 🔄 GitHub Actions Pipeline - Визуальный обзор

## 📊 Архитектура Pipeline

```
┌─────────────────────────────────────────────────────────────────┐
│                     GitHub Repository                            │
│                     (Library MAUI App)                           │
└────────────────┬────────────────────────────────────────────────┘
                 │
                 ├─── Push to main/github_pipeline
                 ├─── Pull Request
                 ├─── Tag push (v*)
                 └─── Manual trigger
                 │
                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                    GitHub Actions Workflows                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌────────────────────┐         ┌─────────────────────┐        │
│  │  android-build.yml │         │   pr-check.yml      │        │
│  │  (Main Pipeline)   │         │   (PR Validation)   │        │
│  └────────────────────┘         └─────────────────────┘        │
│           │                                 │                   │
│           ├─ Debug Build ──────────────────┤                   │
│           │  (commits/PRs)                  │                   │
│           │                                 │                   │
│           └─ Release Build                  │                   │
│              (tags only)                    │                   │
│                                                                  │
└────────────┬────────────────────────────────┬───────────────────┘
             │                                 │
             ▼                                 ▼
┌─────────────────────┐           ┌──────────────────────┐
│   Debug APK         │           │   PR Check Result    │
│   (Artifact)        │           │   (Status Check)     │
│   7 days retention  │           └──────────────────────┘
└─────────────────────┘
             │
             │ (только для tags)
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│              Release Process (Tags only)                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Decode Keystore from GitHub Secrets                         │
│     ↓                                                            │
│  2. Build & Sign APK with Release config                        │
│     ↓                                                            │
│  3. Upload Signed APK as Artifact (90 days)                     │
│     ↓                                                            │
│  4. Create GitHub Release                                       │
│     ↓                                                            │
│  5. Attach APK to Release                                       │
│     ↓                                                            │
│  6. Cleanup Keystore from runner                                │
│                                                                  │
└────────────┬────────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────────┐
│                    GitHub Release                                │
│                                                                  │
│  📦 ru.rapaner.library-Signed.apk                               │
│  📝 Auto-generated release notes                                │
│  🏷️  Version tag (e.g., v1.13)                                  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## 🔀 Workflow Triggers

```
┌─────────────────┐
│  GitHub Event   │
└────────┬────────┘
         │
         ├─────────────────────────────────────────────────────────┐
         │                                                          │
         ▼                                                          ▼
┌──────────────────────┐                              ┌──────────────────────┐
│  Push to branch      │                              │  Tag Push (v*)       │
│  (main/pipeline)     │                              │  (e.g., v1.13)       │
└──────────┬───────────┘                              └──────────┬───────────┘
           │                                                     │
           ▼                                                     ▼
    ┌──────────────┐                                    ┌──────────────────┐
    │ Debug Build  │                                    │  Release Build   │
    │ + PR Check   │                                    │  + Sign          │
    └──────────────┘                                    │  + GitHub Release│
           │                                            └──────────────────┘
           ▼                                                     │
    ┌──────────────┐                                            ▼
    │ APK Artifact │                                   ┌──────────────────┐
    │ (7 days)     │                                   │ APK + Release    │
    └──────────────┘                                   │ (permanent)      │
                                                       └──────────────────┘
         │
         ▼
┌──────────────────────┐
│  Pull Request        │
│  to main             │
└──────────┬───────────┘
           │
           ▼
    ┌──────────────┐
    │  PR Check    │
    │  Build test  │
    └──────────────┘
           │
           ▼
    ┌──────────────┐
    │ Status Check │
    │ ✓ or ✗       │
    └──────────────┘
```

## 🔐 Secrets Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                   GitHub Secrets (Settings)                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  🔑 ANDROID_KEYSTORE_BASE64                                     │
│     └─ Base64 encoded library.keystore                          │
│                                                                  │
│  🔑 ANDROID_KEY_PASSWORD                                        │
│     └─ Password for the signing key                             │
│                                                                  │
│  🔑 ANDROID_STORE_PASSWORD                                      │
│     └─ Password for the keystore                                │
│                                                                  │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         │ (только для Release builds)
                         │
                         ▼
              ┌──────────────────────┐
              │  Workflow Runner     │
              │  (Windows VM)        │
              └──────────┬───────────┘
                         │
                         ├─ 1. Decode Base64 → library.keystore
                         ├─ 2. Use for signing
                         └─ 3. Delete after completion
```

## 📦 Build Process Details

### Debug Build (Commits/PRs)

```
1. Checkout Code
   ↓
2. Setup .NET 9.0
   ↓
3. Install MAUI Workload
   ↓
4. Restore NuGet Packages
   ├─ Library.Core.csproj
   └─ Library.csproj
   ↓
5. Build Debug APK
   └─ dotnet build -c Debug -f net9.0-android
   ↓
6. Upload Artifact
   └─ Expires in 7 days
```

### Release Build (Tags)

```
1. Checkout Code
   ↓
2. Setup .NET 9.0
   ↓
3. Install MAUI Workload
   ↓
4. Restore NuGet Packages
   ├─ Library.Core.csproj
   └─ Library.csproj
   ↓
5. Decode Keystore
   └─ Base64 → library.keystore
   ↓
6. Build & Sign Release APK
   ├─ dotnet publish -c Release
   ├─ /p:AndroidKeyStore=true
   ├─ /p:AndroidSigningKeyStore=library.keystore
   ├─ /p:AndroidSigningKeyAlias=myappalias
   └─ /p:AndroidSigningKeyPass=${{ secrets }}
   ↓
7. Upload Artifact
   └─ Expires in 90 days
   ↓
8. Create GitHub Release
   ├─ Attach Signed APK
   └─ Generate Release Notes
   ↓
9. Cleanup Keystore
   └─ Delete from runner
```

## 📂 File Structure

```
.github/
├── workflows/
│   ├── android-build.yml          [Main] Release & Debug builds
│   └── pr-check.yml               [Helper] PR validation
│
├── scripts/
│   ├── create-keystore.ps1        [Tool] Create new keystore
│   ├── convert-keystore.ps1       [Tool] Convert to Base64
│   └── README.md                  [Docs] Scripts documentation
│
├── ISSUE_TEMPLATE/
│   └── build-issue.md             [Template] Report build issues
│
├── README.md                      [Docs] Quick start guide
├── SETUP_SECRETS.md               [Docs] Secrets setup
├── DEPLOYMENT_GUIDE.md            [Docs] Full deployment guide
├── REMOVE_KEYSTORE.md             [Docs] Security: Remove keystore
├── GITHUB_ACTIONS_SUMMARY.md      [Docs] Quick reference
└── PIPELINE_OVERVIEW.md           [Docs] This file
```

## 🎯 Use Cases

### Case 1: Development (Feature Branch)

```
Developer → Commit → Push to feature branch
                           ↓
                    (No workflow triggered)
                           ↓
                    Create PR to main
                           ↓
                    pr-check.yml runs
                           ↓
                    Build succeeds/fails
                           ↓
                    Merge if passed
```

### Case 2: Regular Commit to Main

```
Developer → Commit → Push to main
                           ↓
                    android-build.yml runs
                           ↓
                    Debug APK created
                           ↓
                    Upload as artifact (7 days)
```

### Case 3: Creating a Release

```
Developer → Update version → Commit → Push
                                        ↓
                                   Create tag v1.14
                                        ↓
                                   Push tag
                                        ↓
                               android-build.yml runs
                                        ↓
                            Release APK created & signed
                                        ↓
                            GitHub Release published
                                        ↓
                               APK ready for download
```

## 📊 Timeline Example

```
Time    Event                    Workflow           Duration    Result
─────────────────────────────────────────────────────────────────────
09:00   Push to main            android-build.yml   ~10 min    Debug APK
09:15   Create PR               pr-check.yml        ~8 min     Status ✓
09:30   Tag v1.13 pushed        android-build.yml   ~12 min    Release
09:45   Download from Release   -                   -          APK file
```

## 🔄 CI/CD Best Practices (Implemented)

✅ **Automated Testing**
- Build validation on every PR
- Prevent broken code from merging

✅ **Security**
- Secrets stored in GitHub Secrets (encrypted)
- Keystore never committed to repo
- Automatic cleanup after use

✅ **Artifact Management**
- Debug builds: 7 days retention
- Release builds: 90 days retention
- Tagged releases: permanent

✅ **Versioning**
- Semantic versioning (vX.Y.Z)
- Automatic version from tags
- Version in Library.csproj

✅ **Documentation**
- Complete setup guides
- Troubleshooting tips
- Helper scripts

## 🚦 Status Indicators

| Indicator | Meaning |
|-----------|---------|
| 🟢 Green checkmark | Build successful |
| 🔴 Red X | Build failed |
| 🟡 Yellow circle | Build in progress |
| ⚪ Gray circle | Build queued |

## 📈 Metrics & Monitoring

### What to Monitor:

1. **Build Success Rate**
   - Target: >95%
   - Check in Actions tab

2. **Build Duration**
   - Debug: ~8-10 minutes
   - Release: ~12-15 minutes

3. **Artifact Size**
   - Typical APK: 20-50 MB
   - Monitor for size increases

4. **Failed Builds**
   - Investigate immediately
   - Check logs for root cause

## 🔧 Troubleshooting Flow

```
Build Failed?
     │
     ├─→ Check Logs in Actions
     │        │
     │        ├─→ Missing Secrets? → Setup Secrets
     │        ├─→ Code Error? → Fix Code
     │        ├─→ Config Error? → Fix .csproj
     │        └─→ Runner Error? → Retry or Report
     │
     └─→ Still failing? → Create Issue
```

## 🎓 Learning Path

1. **Beginner**: Setup secrets, create first release
2. **Intermediate**: Customize workflow, add tests
3. **Advanced**: Optimize build time, add deployment stages

## 📞 Quick Help

| Problem | Solution Doc |
|---------|--------------|
| First time setup | [SETUP_SECRETS.md](SETUP_SECRETS.md) |
| Create release | [DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md) |
| Keystore issues | [REMOVE_KEYSTORE.md](REMOVE_KEYSTORE.md) |
| Script help | [scripts/README.md](scripts/README.md) |
| Quick reference | [GITHUB_ACTIONS_SUMMARY.md](GITHUB_ACTIONS_SUMMARY.md) |

---

**Легенда символов:**
- 📦 Артефакт/Пакет
- 🔑 Секрет/Ключ
- 📝 Документация
- 🔄 Процесс
- ✅ Успех
- ❌ Ошибка
- 🏷️ Тег/Версия

