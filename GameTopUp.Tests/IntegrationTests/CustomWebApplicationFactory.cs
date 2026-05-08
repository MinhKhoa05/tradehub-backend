using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Authentication;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using GameTopUp.DAL;

namespace GameTopUp.Tests.IntegrationTests
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

                // Use a shared memory database to allow multiple connections to see the same data
                const string connectionString = "Data Source=test_db;Mode=Memory;Cache=Shared";
                
                // Keep one connection open to prevent the database from being destroyed
                _connection = new SqliteConnection(connectionString);
                _connection.Open();
                _connection.Execute("PRAGMA busy_timeout = 5000;");

                // Add DatabaseContext: each request gets its own connection to the shared DB
                services.AddScoped<DatabaseContext>(sp => 
                {
                    var conn = new SqliteConnection(connectionString);
                    conn.Open();
                    conn.Execute("PRAGMA busy_timeout = 5000;");
                    return new DatabaseContext(conn);
                });

                // Create the database schema directly on the connection
                _connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT NOT NULL,
                        email TEXT NOT NULL UNIQUE,
                        password_hash TEXT NOT NULL,
                        role INTEGER DEFAULT 0,
                        is_active INTEGER DEFAULT 1,
                        created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        updated_at TEXT DEFAULT CURRENT_TIMESTAMP
                    );
                    CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);
                    CREATE INDEX IF NOT EXISTS idx_users_created ON users(created_at);

                    CREATE TABLE IF NOT EXISTS wallets (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER NOT NULL UNIQUE,
                        balance REAL DEFAULT 0,
                        created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        updated_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE
                    );
                    CREATE INDEX IF NOT EXISTS idx_wallets_user ON wallets(user_id);

                    CREATE TABLE IF NOT EXISTS games (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        image_url TEXT,
                        is_active INTEGER DEFAULT 1,
                        created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        updated_at TEXT DEFAULT CURRENT_TIMESTAMP
                    );

                    CREATE TABLE IF NOT EXISTS game_packages (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        image_url TEXT,
                        normalized_name TEXT NOT NULL,
                        game_id INTEGER NOT NULL,
                        sale_price REAL NOT NULL,
                        original_price REAL NOT NULL,
                        import_price REAL NOT NULL,
                        stock_quantity INTEGER NOT NULL DEFAULT 0,
                        is_active INTEGER DEFAULT 1,
                        created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        updated_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(game_id) REFERENCES games(id) ON DELETE CASCADE
                    );
                    CREATE INDEX IF NOT EXISTS idx_packages_lookup ON game_packages(game_id, is_active);
                    CREATE INDEX IF NOT EXISTS idx_packages_normalized ON game_packages(normalized_name);

                    CREATE TABLE IF NOT EXISTS game_accounts (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER NOT NULL,
                        game_id INTEGER NOT NULL,
                        name TEXT NOT NULL,
                        account_identifier TEXT NOT NULL,
                        server TEXT,
                        description TEXT,
                        is_default INTEGER DEFAULT 0,
                        created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE,
                        FOREIGN KEY(game_id) REFERENCES games(id) ON DELETE CASCADE
                    );
                    CREATE INDEX IF NOT EXISTS idx_accounts_user_sort ON game_accounts(user_id, is_default, created_at);

                    CREATE TABLE IF NOT EXISTS wallet_transactions (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER NOT NULL,
                        amount REAL NOT NULL,
                        balance_before REAL NOT NULL,
                        balance_after REAL NOT NULL,
                        type INTEGER NOT NULL,
                        description TEXT,
                        order_id INTEGER,
                        created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE,
                        FOREIGN KEY(order_id) REFERENCES orders(id) ON DELETE SET NULL
                    );
                    CREATE INDEX IF NOT EXISTS idx_tx_user_sort ON wallet_transactions(user_id, created_at);

                    CREATE TABLE IF NOT EXISTS orders (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER NOT NULL,
                        game_account_info TEXT NOT NULL,
                        game_package_id INTEGER NOT NULL,
                        unit_price REAL NOT NULL,
                        quantity INTEGER NOT NULL,
                        assign_to INTEGER,
                        assign_at TEXT,
                        status INTEGER NOT NULL,
                        created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        updated_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        is_pending INTEGER GENERATED ALWAYS AS (CASE WHEN status = 1 THEN 1 ELSE NULL END) STORED,
                        FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE,
                        FOREIGN KEY(game_package_id) REFERENCES game_packages(id) ON DELETE CASCADE
                    );
                    CREATE UNIQUE INDEX IF NOT EXISTS idx_one_pending_per_user ON orders(user_id, is_pending);
                    CREATE INDEX IF NOT EXISTS idx_orders_user_sort ON orders(user_id, created_at);
                    CREATE INDEX IF NOT EXISTS idx_orders_status ON orders(status);

                    CREATE TABLE IF NOT EXISTS order_history (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        order_id INTEGER NOT NULL,
                        from_status INTEGER NOT NULL,
                        to_status INTEGER NOT NULL,
                        note TEXT,
                        action_by INTEGER NOT NULL,
                        is_admin INTEGER DEFAULT 0,
                        created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(order_id) REFERENCES orders(id) ON DELETE CASCADE
                    );
                    CREATE INDEX IF NOT EXISTS idx_history_order_sort ON order_history(order_id, created_at);
                ");
            });
        }

        public async Task ResetDatabaseAsync()
        {
            if (_connection == null) return;

            // Thứ tự xóa từ bảng con (nhiều FK) đến bảng cha
            var tables = new[] { "order_history", "wallet_transactions", "game_accounts", "orders", "wallets", "users", "game_packages", "games" };
            foreach (var table in tables)
            {
                await _connection.ExecuteAsync($"DELETE FROM {table};");
                await _connection.ExecuteAsync($"DELETE FROM sqlite_sequence WHERE name='{table}';");
            }
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
