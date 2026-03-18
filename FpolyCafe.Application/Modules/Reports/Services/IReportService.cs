using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FpolyCafe.Application.Modules.Reports.Services;

public interface IReportService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TopProductDto>> GetTopSellingProductsAsync(int count = 5, CancellationToken cancellationToken = default);
    Task<IEnumerable<DailyRevenueDto>> GetRevenueReportAsync(int days = 7, CancellationToken cancellationToken = default);
}

public record DashboardSummaryDto(int TotalOrders, decimal TotalRevenue, int TotalProducts, int TotalUsers);
public record TopProductDto(string ProductName, int QuantitySold, decimal Revenue);
public record DailyRevenueDto(DateTime Date, decimal Revenue, int OrderCount);
