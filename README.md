Youdemy (.NET) v2
=================

Kısa açıklama
-------------

Youdemy, ASP.NET Core MVC ile geliştirilmiş Mini-Udemy / LMS platformudur. Amaç, eğitmenlerin kurs oluşturup ders video linkleri ekleyebilmesi; öğrencilerin kurslara abone olup dersleri izleyebilmesi ve ilerlemelerini checkbox ile takip edebilmesidir.

Video dosyaları uygulama içinde veya veritabanında saklanmaz. Eğitmenler videolarını YouTube, Google Drive, Vimeo gibi dış servislerde tutar; uygulama sadece kurs, ders, kullanıcı, abonelik ve ilerleme bilgilerini saklar.

Özellikler
----------

- Eğitmen hesabı ile kurs oluşturma ve düzenleme
- Kurslara ders/video linki ekleme
- YouTube, youtu.be, YouTube Shorts, Google Drive dosya linki, Vimeo ve iframe embed kodu desteği
- Öğrenci hesabı ile kurslara abone olma
- Sadece abone olunan kursun izleme sayfasına erişebilme
- Ders izleme ekranında checkbox ile ilerleme takibi
- Kurs müfredatının ViewComponent ile ayrı bir bileşen olarak gösterilmesi
- Rol tabanlı yetkilendirme: Student, Teacher, Admin
- Eğitmen onay sistemi
- Bildirim ve kullanıcı ayarları ekranları

Teknik detaylar
---------------

- Framework: ASP.NET Core MVC
- Veritabanı: SQLite + Entity Framework Core
- Kimlik doğrulama: Cookie Authentication
- Yetkilendirme: `[Authorize]` attribute'ları ve özel `CourseAuthorizeAttribute`
- ViewComponent: `CourseSyllabusViewComponent`
- Video link dönüştürme: `Helpers/VideoLinkHelper.cs`

Video link mantığı
------------------

Uygulama video dosyasını saklamaz. Ders kaydında yalnızca link tutulur.

Desteklenen örnekler:

```text
https://www.youtube.com/watch?v=VIDEO_ID
https://youtu.be/VIDEO_ID
https://www.youtube.com/shorts/VIDEO_ID
https://drive.google.com/file/d/FILE_ID/view
https://vimeo.com/VIDEO_ID
<iframe src="..."></iframe>
```

Sistem bu linkleri izleme ekranında kullanılabilecek embed/preview formatına dönüştürür.

Kurulum
-------

Gereksinimler:

- .NET SDK
- SQLite desteği Entity Framework Core paketleriyle birlikte projede tanımlıdır

Bağımlılıkları geri yükleme ve derleme:

```powershell
dotnet restore
dotnet build
```

Çalıştırma:

```powershell
dotnet run
```

Uygulama varsayılan olarak `Properties/launchSettings.json` içindeki adreslerden açılır.

Proje yapısı
------------

- `Controllers/`: MVC controller dosyaları
- `Models/`: Entity modelleri ve view modeller
- `Views/`: Razor sayfaları
- `ViewComponents/`: Kurs müfredatı gibi tekrar kullanılabilir bileşenler
- `Filters/`: Özel erişim filtreleri
- `Helpers/`: Yardımcı sınıflar, örn. video link dönüştürücü
- `Data/`: EF Core DbContext ve başlangıç verileri
- `wwwroot/`: CSS, JavaScript ve statik dosyalar

Ödev gereksinimleri ile eşleşme
-------------------------------

- Eğitmen kurs oluşturabilir.
- Eğitmen ders/video linki ekleyebilir.
- Öğrenci kursa abone olabilir.
- Öğrenci izleme ilerlemesini checkbox ile takip edebilir.
- Kurs müfredatı ViewComponent ile gösterilir.
- `Authorize` ve özel `CourseAuthorizeAttribute` ile sadece abone olunan kursa erişim sağlanır.

Doğrulama
---------

Projeyi kontrol etmek için:

```powershell
dotnet build
dotnet test
```

Son kontrolde `dotnet build` komutu `0 Uyarı, 0 Hata` ile başarılı tamamlanmıştır. Projede ayrı bir test projesi bulunmadığı için `dotnet test` yalnızca mevcut projeyi doğrulayıp hatasız tamamlanır.

Notlar
-----

- Veritabanı video dosyalarını değil, uygulama verilerini saklar.
- Google Drive videolarının izlenebilmesi için paylaşım ayarlarının öğrencilerin erişebileceği şekilde yapılması gerekir.
- YouTube veya Vimeo videolarında embed izni kapalıysa dış servis iframe oynatmayı engelleyebilir.

Üçüncü taraf lisansları
-----------------------

Üçüncü taraf kütüphaneler `wwwroot/lib/` altında yer alır. Lisans bilgileri için [THIRD-PARTY-LICENSES.md](THIRD-PARTY-LICENSES.md) dosyasına bakın.
