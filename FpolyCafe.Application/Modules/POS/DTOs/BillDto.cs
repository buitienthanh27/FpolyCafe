using System;
using System.Collections.Generic;

namespace FpolyCafe.Application.Modules.POS.DTOs;

public record BillDto(int BillId, DateTime CreatedAt, decimal TotalAmount, string Status, string StaffName, List<BillDetailDto> Details);
public record BillDetailDto(int BillDetailId, int ProductId, string ProductName, int SizeId, string SizeName, int Quantity, decimal Price, string Notes, List<BillDetailToppingDto> Toppings);
public record BillDetailToppingDto(int ToppingId, string ToppingName, decimal Price);
public record CreateBillRequest(int? UserId);
public record AddBillDetailRequest(int BillId, int ProductId, int SizeId, List<int>? ToppingIds, int Quantity, string? Notes);
