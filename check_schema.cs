using System;
using Microsoft.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connectionString = "Server=LAPTOP-FGLD2IP0\\SQLEXPRESS;Database=FpolyCafeDb;Trusted_Connection=True;Encrypt=False";
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                string[] tables = { "Ingredients", "Recipes" };
                foreach (var table in tables)
                {
                    Console.WriteLine($"\n--- Columns for {table} ---");
                    using (SqlCommand command = new SqlCommand($"SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{table}'", connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"{reader["COLUMN_NAME"]} ({reader["DATA_TYPE"]})");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
