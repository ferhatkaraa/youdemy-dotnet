using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Youdemy.Models;
using Youdemy.Helpers;

namespace Youdemy.Data
{
    public static class DbSeeder
    {
        public static void Seed(YoudemyDbContext context)
        {
            context.Database.EnsureCreated();

            // Sadece tablo kontrolü yapılıyor, eğer yeni veritabanıysa tablo doğrudan var olacak.
            // Olası eksik sütunlar için migration uyumluluğu bırakıldı.
            try
            {
                var conn = context.Database.GetDbConnection();
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Users') WHERE name='DisplayName';";
                    var hasDisplay = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    if (hasDisplay == 0)
                    {
                        using var alter = conn.CreateCommand();
                        alter.CommandText = "ALTER TABLE Users ADD COLUMN DisplayName TEXT;";
                        alter.ExecuteNonQuery();
                    }
                }

                using (var cmd2 = conn.CreateCommand())
                {
                    cmd2.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Users') WHERE name='ProfileImageUrl';";
                    var hasProfileImage = Convert.ToInt32(cmd2.ExecuteScalar() ?? 0);
                    if (hasProfileImage == 0)
                    {
                        using var alter2 = conn.CreateCommand();
                        alter2.CommandText = "ALTER TABLE Users ADD COLUMN ProfileImageUrl TEXT DEFAULT '/images/user-default.png';";
                        alter2.ExecuteNonQuery();
                    }
                }

                using (var cmd3 = conn.CreateCommand())
                {
                    cmd3.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Users') WHERE name='IsApproved';";
                    var hasIsApproved = Convert.ToInt32(cmd3.ExecuteScalar() ?? 0);
                    if (hasIsApproved == 0)
                    {
                        using var alter3 = conn.CreateCommand();
                        alter3.CommandText = "ALTER TABLE Users ADD COLUMN IsApproved INTEGER NOT NULL DEFAULT 1;";
                        alter3.ExecuteNonQuery();
                    }
                }

                using (var cmd4 = conn.CreateCommand())
                {
                    cmd4.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Notifications';";
                    var hasNotifications = Convert.ToInt32(cmd4.ExecuteScalar() ?? 0);
                    if (hasNotifications == 0)
                    {
                        using var createNotifications = conn.CreateCommand();
                        createNotifications.CommandText = @"CREATE TABLE Notifications (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            RecipientId INTEGER NOT NULL,
                            SenderId INTEGER,
                            Message TEXT NOT NULL,
                            CreatedAt TEXT NOT NULL,
                            IsRead INTEGER NOT NULL DEFAULT 0,
                            FOREIGN KEY (RecipientId) REFERENCES Users(Id) ON DELETE CASCADE,
                            FOREIGN KEY (SenderId) REFERENCES Users(Id) ON DELETE SET NULL
                        );";
                        createNotifications.ExecuteNonQuery();
                    }
                }

                conn.Close();
            }
            catch
            {
                // Ignore if fails
            }

            // Veritabanı doluysa bir daha seed etme (Kullanıcı verisi varsa)
            if (context.Users.Any())
            {
                return;
            }

            // 1. Admin Ekle
            var admin = new User
            {
                Username = "admin",
                Email = "admin@youdemy.com",
                PasswordHash = PasswordHelper.HashPassword("admin123"),
                Role = UserRole.Admin,
                IsApproved = true,
                CreatedAt = DateTime.UtcNow,
                DisplayName = "Site Yöneticisi",
                ProfileImageUrl = "/images/admin-default.png"
            };
            context.Users.Add(admin);

            // 2. 6 Eğitmen Ekle
            var teachers = new List<User>();
            var teacherNames = new[] { "Ahmet", "Mehmet", "Ayşe", "Fatma", "Can", "Zeynep" };
            for (int i = 0; i < 6; i++)
            {
                var t = new User
                {
                    Username = $"teacher{i+1}",
                    Email = $"teacher{i+1}@youdemy.com",
                    PasswordHash = PasswordHelper.HashPassword("123456"),
                    Role = UserRole.Teacher,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    DisplayName = $"{teacherNames[i]} Hoca",
                    ProfileImageUrl = "/images/user-default.png"
                };
                teachers.Add(t);
            }
            context.Users.AddRange(teachers);

            // 3. 50 Öğrenci Ekle
            var students = new List<User>();
            for (int i = 1; i <= 50; i++)
            {
                var s = new User
                {
                    Username = $"student{i}",
                    Email = $"student{i}@youdemy.com",
                    PasswordHash = PasswordHelper.HashPassword("123456"),
                    Role = UserRole.Student,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-new Random().Next(1, 30)),
                    DisplayName = $"Öğrenci {i}",
                    ProfileImageUrl = "/images/user-default.png"
                };
                students.Add(s);
            }
            context.Users.AddRange(students);

            context.SaveChanges();

            // 4. 10 Kurs Ekle
            var courseDefinitions = new[]
            {
                new {
                    Title = "Python ile Programlamaya Giriş",
                    Desc = "Sıfırdan başlayanlar için Python programlama dilinin temel mantığı, değişkenler, döngüler ve fonksiyonlar.",
                    Img = "https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?w=500",
                    Lessons = new[] {
                        ("Ders 1: Kurulum ve Giriş", "https://www.youtube.com/embed/rfscVS0vtbw"),
                        ("Ders 2: Değişkenler ve Veri Tipleri", "https://www.youtube.com/embed/Z1Yd7upQsXY"),
                        ("Ders 3: Koşullu İfadeler", "https://www.youtube.com/embed/NWbT9S0A6-I")
                    }
                },
                new {
                    Title = "Modern JavaScript ve ES6+",
                    Desc = "Web geliştirmenin temeli olan JavaScript dilinin modern standartları (Arrow functions, Promises, Async/Await).",
                    Img = "https://images.unsplash.com/photo-1579468118864-1b9ea3c0db4a?w=500",
                    Lessons = new[] {
                        ("Ders 1: JavaScript Temelleri", "https://www.youtube.com/embed/hdI2bqOjy3c"),
                        ("Ders 2: Arrow Functions", "https://www.youtube.com/embed/W6NZfCO5SIk"),
                        ("Ders 3: Asenkron Programlama", "https://www.youtube.com/embed/VlXg7aZt8_k")
                    }
                },
                new {
                    Title = "React ile Bileşen Tabanlı Arayüz Geliştirme",
                    Desc = "React kütüphanesi kullanarak dinamik, hızlı ve modern tek sayfa (SPA) uygulamaları geliştirme kursu.",
                    Img = "https://images.unsplash.com/photo-1633356122544-f134324a6cee?w=500",
                    Lessons = new[] {
                        ("Ders 1: React Kurulumu ve JSX", "https://www.youtube.com/embed/Ke90Tje7VS0"),
                        ("Ders 2: State ve Props Yönetimi", "https://www.youtube.com/embed/O6P86uwfdH0"),
                        ("Ders 3: useEffect Hook Kullanımı", "https://www.youtube.com/embed/TNhaISOUy6Q")
                    }
                },
                new {
                    Title = "HTML5 ve CSS3 ile Web Tasarımının Temelleri",
                    Desc = "İnternet sitelerinin iskeletini ve görsel tasarımını oluşturan HTML ve CSS teknolojilerinin detaylı anlatımı.",
                    Img = "https://images.unsplash.com/photo-1531403009284-440f080d1e12?w=500",
                    Lessons = new[] {
                        ("Ders 1: HTML Etiketleri", "https://www.youtube.com/embed/mU6an75xk6w"),
                        ("Ders 2: CSS Seçiciler ve Kutu Modeli", "https://www.youtube.com/embed/yfoY53QXEnI"),
                        ("Ders 3: Flexbox ile Yerleşim", "https://www.youtube.com/embed/1PnVor36_40")
                    }
                },
                new {
                    Title = "Node.js ve Express ile Backend Geliştirme",
                    Desc = "Sunucu taraflı programlama, RESTful API tasarımı ve Express mimarisi ile backend sistemleri oluşturma.",
                    Img = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=500",
                    Lessons = new[] {
                        ("Ders 1: Node.js Çalışma Mantığı", "https://www.youtube.com/embed/TlB_eWDSMt4"),
                        ("Ders 2: Express ile Sunucu Ayarları", "https://www.youtube.com/embed/Oe421EPjeBE"),
                        ("Ders 3: Routing ve Middleware", "https://www.youtube.com/embed/pKd0Rpw7O48")
                    }
                },
                new {
                    Title = "Git ve GitHub ile Versiyon Kontrol Sistemi",
                    Desc = "Yazılım projelerinde versiyon takibi, takım çalışması yönetimi ve GitHub entegrasyonu.",
                    Img = "https://images.unsplash.com/photo-1618401471353-b98afee0b2eb?w=500",
                    Lessons = new[] {
                        ("Ders 1: Git Temel Komutları", "https://www.youtube.com/embed/8JJ101D3knE"),
                        ("Ders 2: Branch Yönetimi ve Merge", "https://www.youtube.com/embed/usdEmeGowbY"),
                        ("Ders 3: Çakışmaları (Conflict) Çözme", "https://www.youtube.com/embed/RGOj5ykw7oo")
                    }
                },
                new {
                    Title = "SQL ve Veritabanı Tasarım İlkeleri",
                    Desc = "İlişkisel veritabanları, SQL sorguları, normalizasyon ve veritabanı optimizasyonu teknikleri.",
                    Img = "https://images.unsplash.com/photo-1544383835-bda2bc66a55d?w=500",
                    Lessons = new[] {
                        ("Ders 1: Veritabanı Nedir? SELECT Sorguları", "https://www.youtube.com/embed/HXV3zeQKqGY"),
                        ("Ders 2: JOIN İşlemleriyle Tablo Birleştirme", "https://www.youtube.com/embed/7S_tz1z_5bA"),
                        ("Ders 3: Veri Ekleme, Güncelleme ve Silme", "https://www.youtube.com/embed/yPu6qV5byu4")
                    }
                },
                new {
                    Title = "Docker ile Konteynerizasyon Teknolojisi",
                    Desc = "Uygulamaların farklı ortamlarda sorunsuz çalışması için Dockerfile, Docker Image ve Container kavramları.",
                    Img = "https://images.unsplash.com/photo-1605745341112-85968b19335b?w=500",
                    Lessons = new[] {
                        ("Ders 1: Docker Kurulumu ve Temel Mimari", "https://www.youtube.com/embed/pTFZFxd4hOI"),
                        ("Ders 2: Dockerfile Yazımı", "https://www.youtube.com/embed/3c-iBn73dDE"),
                        ("Ders 3: Docker Compose ile Çoklu Konteyner", "https://www.youtube.com/embed/fqMOX6JJCEQ")
                    }
                },
                new {
                    Title = "Veri Yapıları ve Algoritma Temelleri",
                    Desc = "Array, Linked List, Stack, Queue gibi veri yapıları ve arama/sıralama algoritmalarının Big O analizi ile incelenmesi.",
                    Img = "/images/course-default.png",
                    Lessons = new[] {
                        ("Ders 1: Big O Notasyonu ve Zaman Karmaşıklığı", "https://www.youtube.com/embed/RBSGKlAboiM"),
                        ("Ders 2: Sıralama Algoritmaları", "https://www.youtube.com/embed/8hly31xKli0"),
                        ("Ders 3: Ağaç (Tree) Veri Yapıları", "https://www.youtube.com/embed/ZdQ50g6HTww")
                    }
                },
                new {
                    Title = "Figma ile UI/UX Tasarım Süreçleri",
                    Desc = "Dijital ürün tasarımında kullanıcı deneyimi (UX) araştırmaları ve Figma aracı ile arayüz (UI) tasarımı prototipleme.",
                    Img = "https://images.unsplash.com/photo-1611532736597-de2d4265fba3?w=500",
                    Lessons = new[] {
                        ("Ders 1: Figma Arayüzü ve Temel Araçlar", "https://www.youtube.com/embed/FTFaQWZBqA8"),
                        ("Ders 2: Bileşenler (Components) ve Auto Layout", "https://www.youtube.com/embed/c9Wg6gOd_IE"),
                        ("Ders 3: Etkileşimli Prototip Oluşturma", "https://www.youtube.com/embed/jwCm93C_xuM")
                    }
                }
            };

            var rand = new Random();
            var addedCourses = new List<Course>();

            for (int i = 0; i < courseDefinitions.Length; i++)
            {
                var def = courseDefinitions[i];
                var teacher = teachers[i % teachers.Count]; // Dağıt

                var course = new Course
                {
                    Title = def.Title,
                    Description = def.Desc,
                    ImageUrl = def.Img,
                    TeacherId = teacher.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(1, 60))
                };
                
                context.Courses.Add(course);
                addedCourses.Add(course);
            }
            context.SaveChanges();

            // 5. Dersleri Ekle
            for (int i = 0; i < courseDefinitions.Length; i++)
            {
                var course = addedCourses[i];
                var def = courseDefinitions[i];

                for (int j = 0; j < def.Lessons.Length; j++)
                {
                    var lessonDef = def.Lessons[j];
                    context.Lessons.Add(new Lesson
                    {
                        Title = lessonDef.Item1,
                        Description = $"{lessonDef.Item1} içeriği ve detaylı anlatımı.",
                        VideoUrl = lessonDef.Item2,
                        Order = j + 1,
                        CourseId = course.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            context.SaveChanges();

            // 6. Rastgele Kayıtlar (Enrollments) Oluştur
            foreach (var s in students)
            {
                // Her öğrenci ortalama 2-4 kursa kayıt olsun
                int enrollCount = rand.Next(2, 5);
                var pickedCourses = addedCourses.OrderBy(x => rand.Next()).Take(enrollCount).ToList();

                foreach (var c in pickedCourses)
                {
                    context.Enrollments.Add(new Enrollment
                    {
                        StudentId = s.Id,
                        CourseId = c.Id,
                        EnrolledAt = DateTime.UtcNow.AddDays(-rand.Next(1, 10))
                    });
                }
            }
            
            context.SaveChanges();
        }
    }
}
