using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Application.Common.Exceptions;
using FpolyCafe.Application.Modules.POS.DTOs;
using FpolyCafe.Domain.Entities;
using FpolyCafe.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.POS.Services;

public class BillService : IBillService
{
    private readonly IAppDbContext _context;

    public BillService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateBillAsync(int? userId, CancellationToken cancellationToken = default)
    {
        if (userId.HasValue && userId.Value != 0)
        {
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId.Value, cancellationToken);
            if (!userExists) throw new NotFoundException("User", userId.Value);
        }

        var bill = new Bill
        {
            UserId = userId ?? 0,
            CreatedAt = DateTime.UtcNow,
            TotalAmount = 0,
            Status = BillStatus.Waiting
        };

        _context.Bills.Add(bill);
        await _context.SaveChangesAsync(cancellationToken);

        return bill.BillId;
    }

    public async Task<BillDto> GetBillByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var bill = await _context.Bills
            .Include(b => b.User)
            .Include(b => b.BillDetails)
                .ThenInclude(bd => bd.Product)
            .Include(b => b.BillDetails)
                .ThenInclude(bd => bd.BillDetailToppings)
            .FirstOrDefaultAsync(b => b.BillId == id, cancellationToken);

        if (bill == null) throw new NotFoundException("Bill", id);

        return MapToDto(bill);
    }

    public async Task<IEnumerable<BillDto>> GetRecentBillsAsync(CancellationToken cancellationToken = default)
    {
        var bills = await _context.Bills
            .Include(b => b.User)
            .Include(b => b.BillDetails)
                .ThenInclude(bd => bd.Product)
            .Include(b => b.BillDetails)
                .ThenInclude(bd => bd.BillDetailToppings)
            .OrderByDescending(b => b.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        return bills.Select(MapToDto);
    }

    public async Task<bool> AddItemToBillAsync(int billId, int productId, int sizeId, List<int>? toppingIds, int quantity, string note, CancellationToken cancellationToken = default)
    {
        var bill = await _context.Bills.FirstOrDefaultAsync(b => b.BillId == billId, cancellationToken);
        if (bill == null) throw new NotFoundException("Bill", billId);
        if (bill.Status != BillStatus.Waiting) throw new BadRequestException("Không thể thêm món vào hóa đơn đã hoàn thành hoặc đã hủy.");

        var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken);
        if (product == null) throw new NotFoundException("Product", productId);

        var size = await _context.Sizes.FirstOrDefaultAsync(s => s.SizeId == sizeId, cancellationToken);
        if (size == null) throw new NotFoundException("Size", sizeId);

        var billDetail = new BillDetail
        {
            BillId = billId,
            ProductId = productId,
            Quantity = quantity,
            HistoricalProductName = product.Name,
            HistoricalPrice = product.Price,
            Notes = note ?? string.Empty,
            SizeId = size.SizeId,
            SizeName = size.SizeName,
            BillDetailToppings = new List<BillDetailTopping>()
        };

        var toppingsPriceSum = 0m;
        if (toppingIds != null && toppingIds.Any())
        {
            var toppings = await _context.Toppings.Where(t => toppingIds.Contains(t.ToppingId)).ToListAsync(cancellationToken);
            foreach (var t in toppings)
            {
                billDetail.BillDetailToppings.Add(new BillDetailTopping
                {
                    ToppingId = t.ToppingId,
                    HistoricalToppingName = t.ToppingName,
                    HistoricalToppingPrice = t.Price,
                    Quantity = 1
                });
                toppingsPriceSum += t.Price;
            }
        }

        _context.BillDetails.Add(billDetail);
        
        bill.TotalAmount += ((product.Price + toppingsPriceSum) * quantity);
        
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateItemInBillAsync(int billDetailId, int quantity, string note, CancellationToken cancellationToken = default)
    {
        var detail = await _context.BillDetails
            .Include(d => d.Bill)
            .Include(d => d.BillDetailToppings)
            .FirstOrDefaultAsync(d => d.BillDetailId == billDetailId, cancellationToken);

        if (detail == null) throw new NotFoundException("BillDetail", billDetailId);
        if (detail.Bill.Status != BillStatus.Waiting) throw new BadRequestException("Không thể chỉnh sửa hóa đơn đã chốt.");

        var toppingPriceSum = detail.BillDetailToppings.Sum(t => t.HistoricalToppingPrice * t.Quantity);
        var unitPrice = detail.HistoricalPrice + toppingPriceSum;

        var priceDifference = (quantity - detail.Quantity) * unitPrice;
        detail.Bill.TotalAmount += priceDifference;

        detail.Quantity = quantity;
        detail.Notes = note ?? string.Empty;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveItemFromBillAsync(int billDetailId, CancellationToken cancellationToken = default)
    {
        var detail = await _context.BillDetails
            .Include(d => d.Bill)
            .Include(d => d.BillDetailToppings)
            .FirstOrDefaultAsync(d => d.BillDetailId == billDetailId, cancellationToken);

        if (detail == null) throw new NotFoundException("BillDetail", billDetailId);
        if (detail.Bill.Status != BillStatus.Waiting) throw new BadRequestException("Không thể xóa món khỏi hóa đơn đã chốt.");

        var toppingPriceSum = detail.BillDetailToppings.Sum(t => t.HistoricalToppingPrice * t.Quantity);
        detail.Bill.TotalAmount -= ((detail.HistoricalPrice + toppingPriceSum) * detail.Quantity);
        
        _context.BillDetails.Remove(detail);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> CheckoutBillAsync(int billId, CancellationToken cancellationToken = default)
    {
        var bill = await _context.Bills
            .Include(b => b.BillDetails)
                .ThenInclude(bd => bd.BillDetailToppings)
            .FirstOrDefaultAsync(b => b.BillId == billId, cancellationToken);
            
        if (bill == null) throw new NotFoundException("Bill", billId);
        if (bill.Status != BillStatus.Waiting) throw new BadRequestException("Hóa đơn không ở trạng thái chờ.");

        // Inventory Deduction Logic
        foreach (var detail in bill.BillDetails)
        {
            // 1. Deduct Product Recipe Ingredients
            var recipes = await _context.Recipes
                .Include(r => r.Ingredient)
                .Where(r => r.ProductId == detail.ProductId && r.SizeId == detail.SizeId)
                .ToListAsync(cancellationToken);

            foreach (var recipe in recipes)
            {
                if (recipe.Ingredient != null)
                {
                    recipe.Ingredient.StockQuantity -= (recipe.QuantityNeeded * detail.Quantity);
                }
            }

            // 2. Deduct Topping Ingredients
            foreach (var billTopping in detail.BillDetailToppings)
            {
                var toppingDef = await _context.Toppings
                    .Include(t => t.Ingredient)
                    .FirstOrDefaultAsync(t => t.ToppingId == billTopping.ToppingId, cancellationToken);

                if (toppingDef?.Ingredient != null)
                {
                    toppingDef.Ingredient.StockQuantity -= (toppingDef.QuantityNeeded * billTopping.Quantity * detail.Quantity);
                }
            }
        }

        bill.Status = BillStatus.Finished;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> CancelBillAsync(int billId, CancellationToken cancellationToken = default)
    {
        var bill = await _context.Bills.FirstOrDefaultAsync(b => b.BillId == billId, cancellationToken);
        if (bill == null) throw new NotFoundException("Bill", billId);
        if (bill.Status != BillStatus.Waiting) throw new BadRequestException("Chỉ có thể hủy hóa đơn đang chờ.");

        bill.Status = BillStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private BillDto MapToDto(Bill bill)
    {
        return new BillDto(
            bill.BillId,
            bill.CreatedAt,
            bill.TotalAmount,
            bill.Status.ToString(),
            bill.User?.FullName ?? "Khách Vãng Lai",
            bill.BillDetails.Select(d => new BillDetailDto(
                d.BillDetailId,
                d.ProductId,
                d.HistoricalProductName,
                d.SizeId,
                d.SizeName,
                d.Quantity,
                d.HistoricalPrice,
                d.Notes,
                d.BillDetailToppings?.Select(t => new BillDetailToppingDto(
                    t.ToppingId,
                    t.HistoricalToppingName,
                    t.HistoricalToppingPrice
                )).ToList() ?? new List<BillDetailToppingDto>()
            )).ToList()
        );
    }
}
