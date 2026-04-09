using System;

namespace FpolyCafe.Application.Modules.Promotions.DTOs;

public record PromotionDto(
    int PromotionId,
    string? Name,
    string Code,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    decimal? MinimumOrderAmount,
    DateTime StartsAt,
    DateTime EndsAt,
    string Status,
    bool IsActive
);

public record CreatePromotionDto(
    string? Name,
    string Code,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    decimal? MinimumOrderAmount,
    DateTime StartsAt,
    DateTime EndsAt
);

public record UpdatePromotionDto(
    string? Name,
    string Code,
    string? Description,
    string DiscountType,
    decimal DiscountValue,
    decimal? MinimumOrderAmount,
    DateTime StartsAt,
    DateTime EndsAt,
    bool IsActive
);
