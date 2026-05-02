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
                    CREATE TABLE users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT NOT NULL,
                        email TEXT NOT NULL UNIQUE,
                        password_hash TEXT NOT NULL,
                        balance REAL DEFAULT 0,
                        role INTEGER DEFAULT 0,
                        is_active INTEGER DEFAULT 1,
                        created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        updated_at TEXT DEFAULT CURRENT_TIMESTAMP
                    );
                    CREATE INDEX idx_users_username ON users(username);
                    CREATE INDEX idx_users_created ON users(created_at);

                    CREATE TABLE games (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        image_url TEXT,
                        is_active INTEGER DEFAULT 1,
                        created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        updated_at TEXT DEFAULT CURRENT_TIMESTAMP
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
                        created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        updated_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(game_id) REFERENCES games(id) ON DELETE CASCADE
                    );
                    CREATE INDEX idx_packages_lookup ON game_packages(game_id, is_active);
                    CREATE INDEX idx_packages_normalized ON game_packages(normalized_name);

                    CREATE TABLE game_accounts (
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
                    CREATE INDEX idx_accounts_user_sort ON game_accounts(user_id, is_default, created_at);

                    CREATE TABLE cart_items (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER NOT NULL,
                        game_package_id INTEGER NOT NULL,
                        quantity INTEGER NOT NULL DEFAULT 1,
                        FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE,
                        FOREIGN KEY(game_package_id) REFERENCES game_packages(id) ON DELETE CASCADE
                    );
                    CREATE INDEX idx_cart_lookup ON cart_items(user_id, game_package_id);

                    CREATE TABLE wallet_transactions (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER NOT NULL,
                        amount REAL NOT NULL,
                        balance_after REAL NOT NULL,
                        type INTEGER NOT NULL,
                        description TEXT,
                        created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE
                    );
                    CREATE INDEX idx_tx_user_sort ON wallet_transactions(user_id, created_at);

                    CREATE TABLE orders (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER NOT NULL,
                        game_account_info TEXT NOT NULL,
                        wallet_transaction_id INTEGER,
                        game_package_id INTEGER NOT NULL,
                        unit_price REAL NOT NULL,
                        quantity INTEGER NOT NULL,
                        assign_to INTEGER,
                        assign_at TEXT,
                        status INTEGER NOT NULL,
                        created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        updated_at TEXT DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(user_id) REFERENCES users(id) ON DELETE CASCADE,
                        FOREIGN KEY(game_package_id) REFERENCES game_packages(id) ON DELETE CASCADE,
                        FOREIGN KEY(wallet_transaction_id) REFERENCES wallet_transactions(id) ON DELETE SET NULL
                    );
                    CREATE INDEX idx_orders_user_sort ON orders(user_id, created_at);
                    CREATE INDEX idx_orders_status ON orders(status);

                    CREATE TABLE order_history (
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
                    CREATE INDEX idx_history_order_sort ON order_history(order_id, created_at);
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
