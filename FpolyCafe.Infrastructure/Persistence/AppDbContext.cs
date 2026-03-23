using FpolyCafe.Domain.Entities;
using FpolyCafe.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Size> Sizes { get; set; } = null!;
    public DbSet<ProductSize> ProductSizes { get; set; } = null!;
    public DbSet<Topping> Toppings { get; set; } = null!;
    public DbSet<Bill> Bills { get; set; } = null!;
    public DbSet<BillDetail> BillDetails { get; set; } = null!;
    public DbSet<BillDetailTopping> BillDetailToppings { get; set; } = null!;
    public DbSet<Ingredient> Ingredients { get; set; } = null!;
    public DbSet<Recipe> Recipes { get; set; } = null!;
    public DbSet<InventoryReceipt> InventoryReceipts { get; set; } = null!;
    public DbSet<InventoryReceiptDetail> InventoryReceiptDetails { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;
    public DbSet<AttendanceBreak> AttendanceBreaks { get; set; } = null!;
    public DbSet<SalaryRule> SalaryRules { get; set; } = null!;
    public DbSet<AttendanceAdjustment> AttendanceAdjustments { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<MonthlyPayroll> MonthlyPayrolls { get; set; } = null!;
    public DbSet<MonthlyPayrollDetail> MonthlyPayrollDetails { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
