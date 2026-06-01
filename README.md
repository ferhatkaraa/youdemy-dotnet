Youdemy (.NET) v2
===================

Kısa açıklama
-------------

Bu proje, eğitim amaçlı bir ASP.NET web uygulamasıdır. Ders yönetimi, kullanıcı kimlik doğrulama, bildirimler ve ayarlar gibi temel özellikleri içerir.

Hızlı başlangıç
--------------

- Gereksinimler: .NET SDK (en az .NET 6/7/8 uyumluluğunu proje ayarlarına göre kullanın), `msbuild` veya `dotnet` komutları.
- Derleme:

```powershell
msbuild /t:build
# veya
dotnet build
```

- Çalıştırma:

```powershell
dotnet run
```

Proje yapısı (önemli klasörler)
--------------------------------

- Controllers/: MVC denetleyicileri (ör. `HomeController`, `CoursesController`)
- Views/: Razor görünümleri ve paylaşılan layout
- Models/: Veri modelleri ve view modeller
- Data/: Veritabanı bağlamı (`YoudemyDbContext`) ve seeding
- wwwroot/: Statik varlıklar (css, js, üçüncü taraf kütüphaneler)

Üçüncü taraf lisansları
-----------------------

Üçüncü taraf kütüphaneler `wwwroot/lib/` altında yer alır. Lisans bilgileri için [THIRD-PARTY-LICENSES.md](THIRD-PARTY-LICENSES.md) dosyasına bakın.

Geliştirme notları
------------------

- Geliştirme ortamı ayarları: `appsettings.Development.json` ve `Properties/launchSettings.json`
- Veri tabanı başlatma: `Data/DbSeeder.cs`

Sorunlar ve katkı
-----------------

Issue açmak ve katkıda bulunmak için repository üzerinden PR gönderin. Küçük değişiklikler için önce issue açıp tartışma başlatılması önerilir.
