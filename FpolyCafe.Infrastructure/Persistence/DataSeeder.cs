using System;
using System.Linq;
using FpolyCafe.Domain.Entities;
using FpolyCafe.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FpolyCafe.Infrastructure.Persistence;

public static class DataSeeder
{
    public static void SeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Tự động migrate
        context.Database.Migrate();

        if (!context.Users.Any())
        {
            context.Users.Add(new User
            {
                Username = "admin",
                // Giả lập mật khẩu "admin123", sau này sẽ cấu hình BCrypt
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FullName = "Administrator",
                Role = RoleType.Admin,
                IsActive = true
            });

            context.Users.Add(new User
            {
                Username = "staff1",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("staff123"),
                FullName = "Nhân viên Bán Hàng 1",
                Role = RoleType.Staff,
                IsActive = true
            });

            context.SaveChanges();
        }

        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Cà Phê" },
                new Category { Name = "Trà Sữa" },
                new Category { Name = "Nước Ép" }
            );

            context.SaveChanges();
        }

        if (!context.Sizes.Any())
        {
            context.Sizes.AddRange(
                new Size { SizeName = "M" },
                new Size { SizeName = "L" }
            );

            context.SaveChanges();
        }

        if (!context.Products.Any())
        {
            var coffeeCat = context.Categories.First(c => c.Name == "Cà Phê");
            var teaCat = context.Categories.First(c => c.Name == "Trà Sữa");

            context.Products.AddRange(
                new Product { Name = "Cà Phê Đen", CategoryId = coffeeCat.CategoryId, Price = 25000, IsActive = true },
                new Product { Name = "Cà Phê Sữa", CategoryId = coffeeCat.CategoryId, Price = 29000, IsActive = true },
                new Product { Name = "Bạc Xỉu", CategoryId = coffeeCat.CategoryId, Price = 32000, IsActive = true },
                new Product { Name = "Trà Sữa Trân Châu", CategoryId = teaCat.CategoryId, Price = 35000, IsActive = true }
            );

            context.SaveChanges();
        }
    }
}
