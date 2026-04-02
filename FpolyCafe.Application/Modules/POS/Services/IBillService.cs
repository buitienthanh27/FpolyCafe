using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FpolyCafe.Application.Modules.POS.Services;

public interface IBillService
{
    Task<BillDto> GetBillByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<BillDto>> GetRecentBillsAsync(CancellationToken cancellationToken = default);
    Task<int> CreateBillAsync(int? userId, CancellationToken cancellationToken = default);
    Task<bool> AddItemToBillAsync(int billId, int productId, int sizeId, List<int>? toppingIds, int quantity, string note, CancellationToken cancellationToken = default);
    Task<bool> UpdateItemInBillAsync(int billDetailId, int quantity, string note, CancellationToken cancellationToken = default);
    Task<bool> RemoveItemFromBillAsync(int billDetailId, CancellationToken cancellationToken = default);
    Task<bool> CheckoutBillAsync(int billId, CancellationToken cancellationToken = default);
    Task<bool> CancelBillAsync(int billId, CancellationToken cancellationToken = default);
}

public record BillDto(int BillId, DateTime OrderDate, decimal TotalAmount, string Status, string CustomerName, IEnumerable<BillDetailDto> Items);
public record BillDetailDto(int BillDetailId, int ProductId, string ProductName, int SizeId, string SizeName, int Quantity, decimal Price, string Note, IEnumerable<BillDetailToppingDto> Toppings);
public record BillDetailToppingDto(int ToppingId, string ToppingName, decimal Price);

public record CreateBillRequestDto(int? UserId);
public record AddBillItemRequestDto(int ProductId, int SizeId, List<int>? ToppingIds, int Quantity, string? Note);
public record UpdateBillItemRequestDto(int Quantity, string? Note);
