using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FpolyCafe.Infrastructure.Persistence;

namespace FpolyCafe.Infrastructure.Persistence
{
    public static class DbInspector
    {
        public static void Inspect(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var conn = context.Database.GetDbConnection();
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'InventoryReceipts'";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read()) Console.WriteLine("COL: " + reader[0]);
                    }
                }
            }
        }
    }
}
