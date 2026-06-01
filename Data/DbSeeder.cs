using System;
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

            // Ensure new columns exist in existing SQLite DBs (added in recent model changes)
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
                // If automatic migration fails, continue — seed will attempt to run and errors will surface.
            }

            var hasAnyUsers = context.Users.Any();

            // Ensure admin account exists (only admin allowed)
            if (!context.Users.Any(u => u.Role == UserRole.Admin))
            {
                var admin = new User
                {
                    Username = "admin",
                    Email = "admin@youdemy.com",
                    PasswordHash = PasswordHelper.HashPassword("admin123"),
                    Role = UserRole.Admin,
                    CreatedAt = DateTime.UtcNow,
                    DisplayName = "Site Yöneticisi",
                    ProfileImageUrl = "/images/admin-default.png"
                };

                context.Users.Add(admin);
                context.SaveChanges();
            }

            if (hasAnyUsers)
            {
                return; // Database already contains users, leave existing data intact
            }

            var teacher = new User
            {
                Username = "john_teacher",
                Email = "teacher@youdemy.com",
                PasswordHash = PasswordHelper.HashPassword("Password123"),
                Role = UserRole.Teacher,
                CreatedAt = DateTime.UtcNow
            };

            var student = new User
            {
                Username = "jane_student",
                Email = "student@youdemy.com",
                PasswordHash = PasswordHelper.HashPassword("Password123"),
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(teacher, student);
            context.SaveChanges();

            // Create Courses
            var courseCSharp = new Course
            {
                Title = "Sıfırdan C# ve .NET Core Programlama",
                Description = "Bu kursta, modern C# dilinin temellerini, nesne yönelimli programlama (OOP) kavramlarını ve .NET platformunun gücünü sıfırdan öğrenerek profesyonel web backend uygulamaları geliştirmeyi öğreneceksiniz.",
                ImageUrl = "/images/csharp.png",
                TeacherId = teacher.Id,
                CreatedAt = DateTime.UtcNow
            };

            var courseWeb = new Course
            {
                Title = "Modern Web Geliştirme: HTML, CSS ve JavaScript",
                Description = "Hiçbir harici kütüphane (Bootstrap, Tailwind vb.) kullanmadan, tarayıcıların saf gücünü keşfedin! Flexbox, Grid, CSS Değişkenleri ve modern JavaScript ile büyüleyici, performansı yüksek ve responsive kullanıcı arayüzleri geliştirin.",
                ImageUrl = "/images/webdev.png",
                TeacherId = teacher.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Courses.AddRange(courseCSharp, courseWeb);
            context.SaveChanges();

            // Create Lessons for C# Course
            var lessonsCSharp = new[]
            {
                new Lesson
                {
                    Title = "1. C# Nedir? Kurulum ve İlk Konsol Uygulaması",
                    Description = ".NET ortamını tanıyacak, SDK kurulumunu yapacak ve ilk C# kodunuzu terminalde çalıştıracaksınız.",
                    VideoUrl = "https://www.youtube.com/embed/gfkTfcpWqAY",
                    Order = 1,
                    CourseId = courseCSharp.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Lesson
                {
                    Title = "2. Değişkenler, Veri Tipleri ve Tip Dönüşümleri",
                    Description = "Verileri bellekte nasıl tutacağımızı, değer ve referans tiplerini ve güvenli tür dönüşümlerini inceleyeceğiz.",
                    VideoUrl = "https://www.youtube.com/embed/d3WexzTsd-s",
                    Order = 2,
                    CourseId = courseCSharp.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Lesson
                {
                    Title = "3. Koşul Yapıları ve Döngüler ile Program Akışı",
                    Description = "if-else, switch-case blokları ile for, while ve foreach döngüleriyle algoritmik kontrol akışlarını öğreneceğiz.",
                    VideoUrl = "https://www.youtube.com/embed/wzXjV08kQ9c",
                    Order = 3,
                    CourseId = courseCSharp.Id,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Create Lessons for Web Course
            var lessonsWeb = new[]
            {
                new Lesson
                {
                    Title = "1. Semantik HTML5 ile Sayfa İskeleti Kurmak",
                    Description = "SEO ve erişilebilirlik kurallarına uygun, modern semantik etiketler (header, section, article, footer) ile web sayfası tasarlayacağız.",
                    VideoUrl = "https://www.youtube.com/embed/mU6an75A6UY",
                    Order = 1,
                    CourseId = courseWeb.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Lesson
                {
                    Title = "2. CSS Flexbox ve Responsive Grid Mimarisi",
                    Description = "Sayfa elemanlarını Bootstrap olmadan kolayca hizalamayı, modern Flexbox ve Grid yapılarını ve CSS degişkenlerini öğreneceğiz.",
                    VideoUrl = "https://www.youtube.com/embed/Ox0S5Z1ClV8",
                    Order = 2,
                    CourseId = courseWeb.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Lesson
                {
                    Title = "3. Vanilla JS ile Sayfa Etkileşimi (DOM Manipülasyonu)",
                    Description = "Olay dinleyicileri (event listeners), dinamik element oluşturma ve AJAX istekleri ile web sayfalarımıza hayat katacağız.",
                    VideoUrl = "https://www.youtube.com/embed/y17RuWkWdn8",
                    Order = 3,
                    CourseId = courseWeb.Id,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Lessons.AddRange(lessonsCSharp);
            context.Lessons.AddRange(lessonsWeb);
            context.SaveChanges();
        }
    }
}
