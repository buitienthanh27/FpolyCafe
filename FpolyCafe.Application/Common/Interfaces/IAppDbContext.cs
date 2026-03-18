using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; set; }
    DbSet<Category> Categories { get; set; }
    DbSet<Product> Products { get; set; }
    DbSet<Bill> Bills { get; set; }
    DbSet<BillDetail> BillDetails { get; set; }
    DbSet<Size> Sizes { get; set; }
    DbSet<ProductSize> ProductSizes { get; set; }
    DbSet<Topping> Toppings { get; set; }
    DbSet<BillDetailTopping> BillDetailToppings { get; set; }
    DbSet<Ingredient> Ingredients { get; set; }
    DbSet<Recipe> Recipes { get; set; }
    DbSet<InventoryReceipt> InventoryReceipts { get; set; }
    DbSet<InventoryReceiptDetail> InventoryReceiptDetails { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
