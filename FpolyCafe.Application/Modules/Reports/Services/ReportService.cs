using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FpolyCafe.Application.Common.Interfaces;
using FpolyCafe.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FpolyCafe.Application.Modules.Reports.Services;

public class ReportService : IReportService
{
    private readonly IAppDbContext _context;

    public ReportService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        
        var todayRevenue = await _context.Bills
            .Where(b => b.CreatedAt >= today && b.Status == BillStatus.Finished)
            .SumAsync(b => b.TotalAmount, cancellationToken);

        var todayBills = await _context.Bills
            .CountAsync(b => b.CreatedAt >= today && b.Status == BillStatus.Finished, cancellationToken);

        var totalProducts = await _context.Products.CountAsync(cancellationToken);
        var totalUsers = await _context.Users.CountAsync(cancellationToken);

        return new DashboardSummaryDto(todayBills, todayRevenue, totalProducts, totalUsers);
    }

    public async Task<IEnumerable<TopProductDto>> GetTopSellingProductsAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        var topProducts = await _context.BillDetails
            .Where(bd => bd.Bill.Status == BillStatus.Finished)
            .GroupBy(bd => new { bd.ProductId, bd.HistoricalProductName })
            .Select(g => new TopProductDto(
                g.Key.HistoricalProductName,
                g.Sum(x => x.Quantity),
                 g.Sum(x => x.Quantity * x.HistoricalPrice)
            ))
            .OrderByDescending(x => x.QuantitySold)
            .Take(count)
            .ToListAsync(cancellationToken);

        return topProducts;
    }

    public async Task<IEnumerable<DailyRevenueDto>> GetRevenueReportAsync(int days = 7, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        var report = await _context.Bills
            .Where(b => b.CreatedAt >= startDate && b.Status == BillStatus.Finished)
            .GroupBy(b => b.CreatedAt.Date)
            .Select(g => new DailyRevenueDto(
                g.Key,
                g.Sum(x => x.TotalAmount),
                g.Count()
            ))
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return report;
    }
}
