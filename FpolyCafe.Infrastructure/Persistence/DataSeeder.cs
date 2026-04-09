using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using FpolyCafe.Domain.Entities;
using FpolyCafe.Domain.Enums;

namespace FpolyCafe.Infrastructure.Persistence;

public static class DataSeeder
{
    public static void SeedData(IServiceProvider serviceProvider)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();

            if (!context.Users.Any())
            {
                context.Users.AddRange(
                    new User { Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), FullName = "Nguyễn Quản Lý", Role = RoleType.Admin },
                    new User { Username = "manager1", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), FullName = "Trần Trưởng Ca", Role = RoleType.Manager },
                    new User { Username = "staff1", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), FullName = "Lê Nhân Viên", Role = RoleType.Staff }
                );
                context.SaveChanges();
            }

            if (!context.Customers.Any())
            {
                context.Customers.Add(new Customer { FullName = @"Nguyễn Văn A", PhoneNumber = @"0987654321", RewardPoints = 10, CreatedAt = DateTime.UtcNow });
                context.SaveChanges();
            }

            if (!context.Promotions.Any())
            {
                context.Promotions.Add(new Promotion { Name = @"Giảm giá 10%", Code = @"GIAM10", DiscountType = "Percentage", DiscountValue = 10, StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddMonths(1) });
                context.SaveChanges();
            }
        }
    }
}
