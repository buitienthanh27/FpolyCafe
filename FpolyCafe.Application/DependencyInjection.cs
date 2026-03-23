using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using FpolyCafe.Application.Modules.Auth.Services;
using FpolyCafe.Application.Modules.Categories.Services;
using FpolyCafe.Application.Modules.Products.Services;
using FpolyCafe.Application.Modules.POS.Services;
using FpolyCafe.Application.Modules.Reports.Services;
using FpolyCafe.Application.Modules.Users.Services;
using FpolyCafe.Application.Modules.Attendance.Services;
using FpolyCafe.Application.Modules.Payroll.Services;
using FpolyCafe.Application.Modules.Lookups.Services;
using FpolyCafe.Application.Modules.SalaryRules.Services;
using FpolyCafe.Application.Modules.AuditLogs.Services;

namespace FpolyCafe.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IBillService, BillService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<ISalaryRuleService, SalaryRuleService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
