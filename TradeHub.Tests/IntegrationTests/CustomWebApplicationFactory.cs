using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using TradeHub.DAL;

namespace TradeHub.Tests.IntegrationTests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private SqliteConnection? _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DatabaseContext));
                if (descriptor != null) services.Remove(descriptor);

                // Add Test Auth
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                // Create and open a SQLite connection
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                // Add DatabaseContext using SQLite
                services.AddScoped<DatabaseContext>(sp => new DatabaseContext(_connection!));

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create the database schema
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                    db.Connection.Execute(@"
                        CREATE TABLE games (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            name TEXT NOT NULL,
                            image_url TEXT,
                            is_active INTEGER DEFAULT 1
                        );
                        CREATE TABLE game_packages (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            name TEXT NOT NULL,
                            image_url TEXT,
                            normalized_name TEXT NOT NULL,
                            game_id INTEGER NOT NULL,
                            sale_price REAL NOT NULL,
                            original_price REAL NOT NULL,
                            import_price REAL NOT NULL,
                            package_budget REAL NOT NULL,
                            spent_amount REAL DEFAULT 0,
                            is_active INTEGER DEFAULT 1,
                            created_at TEXT,
                            updated_at TEXT,
                            FOREIGN KEY(game_id) REFERENCES games(id) ON DELETE CASCADE
                        );
                    ");
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection?.Close();
                _connection?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
