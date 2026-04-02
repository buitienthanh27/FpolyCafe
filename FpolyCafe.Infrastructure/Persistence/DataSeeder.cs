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

        context.Database.Migrate();

        if (!context.Users.Any())
        {
            context.Users.AddRange(
                new User
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    FullName = "Administrator",
                    Role = RoleType.Admin,
                    IsActive = true
                },
                new User
                {
                    Username = "staff1",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("staff123"),
                    FullName = "Nhân viên Bán Hàng 1",
                    Role = RoleType.Staff,
                    IsActive = true
                },
                new User
                {
                    Username = "manager1",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    FullName = "Qu?n l? Ca 1",
                    Role = RoleType.Manager,
                    IsActive = true
                });

            context.SaveChanges();
        }

        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Cà Phê", IsActive = true },
                new Category { Name = "Trà S?a", IsActive = true },
                new Category { Name = "Ný?c Ép", IsActive = true });
            context.SaveChanges();
        }

        if (!context.Sizes.Any())
        {
            context.Sizes.AddRange(
                new Size { SizeName = "M" },
                new Size { SizeName = "L" });
            context.SaveChanges();
        }

        if (!context.Ingredients.Any())
        {
            context.Ingredients.AddRange(
                new Ingredient { IngredientName = "Cà phê h?t", Unit = "gram", StockQuantity = 5000 },
                new Ingredient { IngredientName = "S?a ð?c", Unit = "ml", StockQuantity = 3000 },
                new Ingredient { IngredientName = "Trân châu", Unit = "gram", StockQuantity = 4000 });
            context.SaveChanges();
        }

        if (!context.Products.Any())
        {
            var coffeeCat = context.Categories.First(c => c.Name == "Cà Phê");
            var teaCat = context.Categories.First(c => c.Name == "Trà S?a");

            context.Products.AddRange(
                new Product { Name = "Cà Phê Ðen", CategoryId = coffeeCat.CategoryId, Price = 25000, IsActive = true },
                new Product { Name = "Cà Phê S?a", CategoryId = coffeeCat.CategoryId, Price = 29000, IsActive = true },
                new Product { Name = "B?c X?u", CategoryId = coffeeCat.CategoryId, Price = 32000, IsActive = true },
                new Product { Name = "Trà S?a Trân Châu", CategoryId = teaCat.CategoryId, Price = 35000, IsActive = true });
            context.SaveChanges();
        }

        if (!context.Toppings.Any())
        {
            var pearl = context.Ingredients.First(i => i.IngredientName == "Trân châu");
            context.Toppings.AddRange(
                new Topping { ToppingName = "Trân châu ðen", Price = 8000, IngredientId = pearl.IngredientId, QuantityNeeded = 30, IsActive = true },
                new Topping { ToppingName = "Foam kem s?a", Price = 12000, QuantityNeeded = 0, IsActive = true });
            context.SaveChanges();
        }

        if (!context.SalaryRules.Any())
        {
            context.SalaryRules.AddRange(
                new SalaryRule { Role = RoleType.Staff, HourlyRate = 25000, OvertimeRate = 35000, NightShiftMultiplier = 1.2m, StandardHoursPerShift = 8, MaxHoursPerShift = 12, EffectiveFrom = DateTime.UtcNow.Date, IsActive = true },
                new SalaryRule { Role = RoleType.Manager, HourlyRate = 35000, OvertimeRate = 50000, NightShiftMultiplier = 1.3m, StandardHoursPerShift = 8, MaxHoursPerShift = 12, EffectiveFrom = DateTime.UtcNow.Date, IsActive = true },
                new SalaryRule { Role = RoleType.Admin, HourlyRate = 40000, OvertimeRate = 60000, NightShiftMultiplier = 1.3m, StandardHoursPerShift = 8, MaxHoursPerShift = 12, EffectiveFrom = DateTime.UtcNow.Date, IsActive = true });
            context.SaveChanges();
        }
    }
}
