using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Exceptions;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Application.Modules.Promotions.DTOs;
using FpolyCafe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.Promotions.Services;

public class PromotionService : IPromotionService
{
    private readonly IAppDbContext _context;

    public PromotionService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PromotionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var promotions = await _context.Promotions
            .OrderByDescending(p => p.StartDate)
            .ToListAsync(cancellationToken);
            
        return promotions.Select(MapToDto);
    }

    public async Task<IEnumerable<PromotionDto>> GetAvailableAsync(decimal orderAmount, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var promotions = await _context.Promotions
            .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now && p.MinBillAmount.GetValueOrDefault() <= orderAmount)
            .ToListAsync(cancellationToken);

        return promotions.Select(MapToDto);
    }

    public async Task<PromotionDto> ValidateAsync(string code, decimal orderAmount, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var promotion = await _context.Promotions
            .FirstOrDefaultAsync(p => p.Code == code && p.IsActive, cancellationToken);

        if (promotion == null) throw new NotFoundException("Mã giảm giá", code);
        if (promotion.StartDate > now) throw new BadRequestException("Chương trình chưa bắt đầu.");
        if (promotion.EndDate < now) throw new BadRequestException("Chương trình đã kết thúc.");
        if (promotion.MinBillAmount.GetValueOrDefault() > orderAmount) throw new BadRequestException($"Đơn hàng chưa đạt mức tối thiểu ({promotion.MinBillAmount.GetValueOrDefault():N0}đ).");

        return MapToDto(promotion);
    }

    public async Task<PromotionDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.PromotionId == id, cancellationToken);
        if (promotion == null) throw new NotFoundException("Promotion", id);
        return MapToDto(promotion);
    }

    public async Task<PromotionDto> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.Code == code && p.IsActive, cancellationToken);
        if (promotion == null) throw new NotFoundException("Promotion code", code);
        return MapToDto(promotion);
    }

    public async Task<int> CreateAsync(CreatePromotionDto request, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Promotions.AnyAsync(p => p.Code == request.Code, cancellationToken);
        if (exists) throw new BadRequestException("Mã khuyến mãi đã tồn tại.");

        var promotion = new Promotion
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinBillAmount = request.MinimumOrderAmount,
            StartDate = request.StartsAt,
            EndDate = request.EndsAt,
            IsActive = true
        };

        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync(cancellationToken);
        return promotion.PromotionId;
    }

    public async Task<bool> UpdateAsync(int id, UpdatePromotionDto request, CancellationToken cancellationToken = default)
    {
        var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.PromotionId == id, cancellationToken);
        if (promotion == null) throw new NotFoundException("Promotion", id);

        promotion.Name = request.Name;
        promotion.Code = request.Code;
        promotion.Description = request.Description;
        promotion.DiscountType = request.DiscountType;
        promotion.DiscountValue = request.DiscountValue;
        promotion.MinBillAmount = request.MinimumOrderAmount;
        promotion.StartDate = request.StartsAt;
        promotion.EndDate = request.EndsAt;
        promotion.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.PromotionId == id, cancellationToken);
        if (promotion == null) throw new NotFoundException("Promotion", id);

        promotion.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private PromotionDto MapToDto(Promotion p)
    {
        var now = DateTime.UtcNow;
        string status = "active";
        if (!p.IsActive) status = "inactive";
        else if (p.StartDate > now) status = "scheduled";
        else if (p.EndDate < now) status = "expired";

        return new PromotionDto(
            p.PromotionId,
            p.Name,
            p.Code,
            p.Description,
            p.DiscountType,
            p.DiscountValue,
            p.MinBillAmount,
            p.StartDate,
            p.EndDate,
            status,
            p.IsActive
        );
    }
}
