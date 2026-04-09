using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FpolyCafe.Infrastructure.Persistence.AppDbContext>();
    var conn = context.Database.GetDbConnection();
    conn.Open();
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'InventoryReceipts'";
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read()) Console.WriteLine(reader[0]);
        }
    }
}
