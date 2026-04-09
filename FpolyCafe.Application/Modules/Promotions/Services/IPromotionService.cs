using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Modules.Promotions.DTOs;

namespace FpolyCafe.Application.Modules.Promotions.Services;

public interface IPromotionService
{
    Task<IEnumerable<PromotionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PromotionDto>> GetAvailableAsync(decimal orderAmount, CancellationToken cancellationToken = default);
    Task<PromotionDto> ValidateAsync(string code, decimal orderAmount, CancellationToken cancellationToken = default);
    Task<PromotionDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PromotionDto> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreatePromotionDto request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, UpdatePromotionDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
