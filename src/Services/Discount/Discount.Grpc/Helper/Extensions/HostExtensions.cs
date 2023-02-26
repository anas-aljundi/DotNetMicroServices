using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Threading;

namespace Discount.Grpc.Helper.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host, int? retry = 0)
        {
            int retryForAvailability = retry.Value;
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var configuration = services.GetRequiredService<IConfiguration>();
                var logger = services.GetRequiredService<ILogger<TContext>>();
                try
                {
                    logger.LogInformation("Create Postgre SQL DB.");
                    var connection = new NpgsqlConnection(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
                    //open a connection to perform multiple commands
                    connection.Open();
                    using var command = new NpgsqlCommand
                    {
                        Connection = connection
                    };
                    //Drop table if exist once the application starts
                    command.CommandText = "DROP TABLE IF EXISTS Coupon";
                    command.ExecuteNonQuery();

                    //create table
                    command.CommandText = @"CREATE TABLE Coupon(Id SERIAL PRIMARY KEY,
                                            ProductName VARCHAR(24) NOT NULL,
                                            Description TEXT,
                                            Amount INT)";
                    command.ExecuteNonQuery();

                    //insert data
                    command.CommandText = "INSERT INTO Coupon (ProductName, Description, Amount) Values ('IPhone X', 'Iphone Discount', 150)";
                    command.ExecuteNonQuery();
                    command.CommandText = "INSERT INTO Coupon (ProductName, Description, Amount) Values ('Samsyng 20 Plus', 'SamSung Discount', 250)";
                    command.ExecuteNonQuery();

                    logger.LogInformation("Postgre DB created and data inserted");
                }
                catch (NpgsqlException ex)
                {
                    logger.LogError(ex, "An Error occured while Creating Postgre DB");
                    if (retryForAvailability < 50)
                    {
                        retryForAvailability++;
                        Thread.Sleep(2000);
                        MigrateDatabase<TContext>(host, retryForAvailability);
                    }
                }
            }
            return host;
        }
    }
}
