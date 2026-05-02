using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication;
using System.Data.Common;
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

        private class TestDatabaseContext : DatabaseContext
        {
            public TestDatabaseContext(DbConnection connection) : base(connection) { }
            // Do not dispose the shared connection
            public override void Dispose() { }
            public override ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }

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

                // Add DatabaseContext using SQLite (shared connection)
                services.AddScoped<DatabaseContext>(sp => new TestDatabaseContext(_connection!));

                // Create the database schema directly on the connection
                _connection.Execute(@"
                    CREATE TABLE games (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        image_url TEXT,
                        is_active INTEGER DEFAULT 1,
                        created_at TEXT,
                        updated_at TEXT
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
                    CREATE TABLE users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT NOT NULL,
                        email TEXT NOT NULL,
                        password_hash TEXT NOT NULL,
                        balance REAL DEFAULT 0,
                        role INTEGER DEFAULT 0,
                        is_active INTEGER DEFAULT 1,
                        created_at TEXT,
                        updated_at TEXT
                    );
                ");
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
