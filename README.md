# Company Info Tracker

Müşteri firmalarına ait bağlantı bilgilerini (VPN, uygulama URL'leri, uzak masaüstü IP'leri, veritabanı sunucuları ve bunlara ait kullanıcı bilgileri) tek bir yerde tutmak için geliştirilmiş, rol tabanlı yetkilendirmeye sahip bir ASP.NET Core MVC web uygulaması.

> *An ASP.NET Core MVC app for keeping customer companies' connection details (VPN, URLs, remote IPs, database servers) in one place, with role-based access control. UI language: Turkish.*

Bu proje staj dönemimde geliştirdiğim bir iç araçtır.

## Özellikler

- **Kimlik doğrulama ve roller:** ASP.NET Core Identity ile giriş/çıkış; `Admin` ve `Kullanici` rolleri. Hatalı denemelerde hesap geçici olarak kilitlenir.
- **Firma yönetimi:** Firma ekleme, düzenleme, silme. Admin tüm firmaları, normal kullanıcı yalnızca kendisine atanmış firmaları görür.
- **Kullanıcı–firma ataması:** Admin panelinden kullanıcıları firmalara atama/kaldırma (çoka-çok ilişki).
- **Kullanıcı yönetimi:** Admin yeni kullanıcı oluşturabilir ve silebilir.
- **Şifreli saklama:** Firma şifreleri veritabanına ASP.NET Core Data Protection ile şifrelenerek yazılır; anahtarlar da veritabanında (`DataProtectionKeys` tablosu) tutulur.
- **Gizli alanlar:** Şifreler listede bulanık gösterilir; tek tuşla göster/gizle.
- **Hızlı kopyalama:** Tablodaki herhangi bir hücreye çift tıklayarak değeri panoya kopyalama.
- **Sayfalama:** Tüm listelerde sayfa başına 10 kayıt.

## Teknolojiler

- .NET 8 / ASP.NET Core MVC (Razor Views)
- Entity Framework Core 8 (Code-First, migrations)
- ASP.NET Core Identity
- SQL Server 2022 (Docker)
- Bootstrap 5

## Kurulum (macOS / Apple Silicon)

### Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) — macOS **Arm64** kurulum paketini seçin
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) — Apple Silicon'da *Settings → General → "Use Rosetta for x86_64/amd64 emulation"* açık olmalı
- (Önerilen) VS Code + [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) eklentisi

### Çalıştırma

```bash
git clone https://github.com/utkuertac/company-info-tracker.git
cd company-info-tracker

# 1) Rastgele şifrelerle .env dosyasını oluştur (giriş bilgilerini ekrana yazar)
./scripts/setup-env.sh

# 2) SQL Server'ı başlat
docker compose up -d

# 3) Uygulamayı çalıştır (veritabanı ve tablolar ilk açılışta otomatik oluşturulur)
dotnet run
```

Uygulama `http://localhost:5049` adresinde açılır.

### Kullanıcılar

İlk açılışta iki hesap otomatik oluşturulur. Şifreleri `setup-env.sh` üretir ve `.env` dosyasında saklanır:

| Kullanıcı adı | Rol       | Şifre (`.env` içinde)      |
|---------------|-----------|----------------------------|
| `admin`       | Admin     | `SeedUsers__AdminPassword` |
| `kullanici`   | Kullanici | `SeedUsers__UserPassword`  |

### Yapılandırma ve gizli bilgiler

Repoda hiçbir şifre bulunmaz:

- **Yerel geliştirme:** Şifreler git'e eklenmeyen `.env` dosyasındadır. Şablonu [`.env.example`](.env.example) dosyasıdır. Docker Compose ve uygulama aynı dosyayı okur.
- **Sunucu / production:** `.env` yerine ortam değişkenleri veya bir secret manager (Azure Key Vault vb.) kullanın:

  ```bash
  ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=...;"
  SeedUsers__AdminPassword="<güçlü-bir-şifre>"
  ```

- **Güvenlik kilidi:** Bir şifre eksikse ya da hâlâ `.env.example`'daki `CHANGE_ME` değerindeyse uygulama açılmaz ve neyin eksik olduğunu söyler.

## Proje Yapısı

```
├── Controllers/        # Account, Admin, Companies, Home
├── Data/               # AppDbContext ve başlangıç verisi (SeedData)
├── Migrations/         # EF Core migration'ları
├── Models/             # Company, ApplicationUser ve sahip olunan (owned) tipler
│   └── ViewModels/     # Form modelleri
├── Views/              # Razor görünümleri
├── wwwroot/            # Statik dosyalar (CSS, JS, kütüphaneler)
├── scripts/            # setup-env.sh: .env oluşturma
├── .env.example        # Yerel ayar şablonu (gerçek .env git'e girmez)
├── docker-compose.yml  # Yerel SQL Server
└── Program.cs          # Uygulama başlangıcı ve servis kayıtları
```

## Bilinen Kısıtlar

- Şifreleme anahtarları verilerle aynı veritabanında saklanır; bu, veritabanı yedeğini ele geçiren birinin şifreleri çözebileceği anlamına gelir. Gerçek bir ortamda anahtarların ayrıca korunması (ör. `ProtectKeysWithCertificate`, Azure Key Vault) gerekir.
- Firmaya atanmış her kullanıcı o firmanın şifrelerini görebilir; alan bazlı yetkilendirme yoktur.
- Proje bir staj çalışmasıdır ve üretim ortamı için tasarlanmamıştır.

## Lisans

[MIT](LICENSE)
