using Dapper;
using Dommel;
using SqlKata;

namespace TradeHub.DAL
{
    /// <summary>
    /// Database access extensions providing 3 ways of querying:
    /// 
    /// 1. Dapper.Contrib  -> simple CRUD (entity-based)
    /// 2. SqlKata         -> dynamic / composable queries
    /// 3. Raw SQL         -> full control / complex queries
    /// 
    /// NOTE:
    /// This is a HYBRID DAL design.
    /// The goal is flexibility, not enforcing a single ORM.
    /// </summary>
    public static class DatabaseExtensions
    {
        #region --- NHÓM 1: CRUD NHANH (Dapper.Contrib) ---

        /// <summary>
        /// Get entity by primary key.
        /// Use for SIMPLE lookup only.
        /// </summary>
        public static async Task<T?> GetByIdAsync<T>(this DatabaseContext db, object id) where T : class
        {
            await db.EnsureOpenAsync();
            return await db.Connection.GetAsync<T>(id, db.Transaction);
        }

        /// <summary>
        /// Insert entity and return generated identity (if any).
        /// Use for simple inserts (no custom SQL logic).
        /// </summary>
        public static async Task<long> InsertAsync<T>(this DatabaseContext db, T entity) where T : class
        {
            await db.EnsureOpenAsync();
            var id = await db.Connection.InsertAsync(entity, db.Transaction);
            return Convert.ToInt64(id);
        }

        /// <summary>
        /// Update full entity.
        /// Use when updating entire model or tracked entity state.
        /// </summary>
        public static async Task<bool> UpdateAsync<T>(this DatabaseContext db, T entity) where T : class
        {
            await db.EnsureOpenAsync();
            return await db.Connection.UpdateAsync(entity, db.Transaction);
        }

        /// <summary>
        /// Delete entity by model.
        /// Use for simple delete operations.
        /// </summary>
        public static async Task<bool> DeleteAsync<T>(this DatabaseContext db, T entity) where T : class
        {
            await db.EnsureOpenAsync();
            return await db.Connection.DeleteAsync(entity, db.Transaction);
        }

        #endregion

        #region --- NHÓM 2: QUERY LINH HOẠT (SqlKata) ---

        /// <summary>
        /// Create SqlKata query builder for a table.
        /// Use ONLY for dynamic queries (filtering, OR conditions, optional params).
        /// </summary>
        public static Query Table(this DatabaseContext db, string table)
            => new Query(table);

        /// <summary>
        /// Execute SELECT query and return list of results.
        /// Use for dynamic queries built with SqlKata.
        /// </summary>
        public static async Task<List<T>> GetAsync<T>(this DatabaseContext db, Query query)
        {
            await db.EnsureOpenAsync();

            var compiled = db.Compiler.Compile(query);

            return (await db.Connection.QueryAsync<T>(
                compiled.Sql,
                compiled.NamedBindings,
                db.Transaction
            )).AsList();
        }

        /// <summary>
        /// Execute query and return first row or default.
        /// </summary>
        public static async Task<T?> FirstOrDefaultAsync<T>(this DatabaseContext db, Query query)
        {
            await db.EnsureOpenAsync();

            var compiled = db.Compiler.Compile(query);

            return await db.Connection.QueryFirstOrDefaultAsync<T>(
                compiled.Sql,
                compiled.NamedBindings,
                db.Transaction
            );
        }

        /// <summary>
        /// Execute non-query (UPDATE/DELETE) using SqlKata.
        /// </summary>
        public static async Task<int> ExecuteAsync(this DatabaseContext db, Query query)
        {
            await db.EnsureOpenAsync();

            var compiled = db.Compiler.Compile(query);

            return await db.Connection.ExecuteAsync(
                compiled.Sql,
                compiled.NamedBindings,
                db.Transaction
            );
        }

        /// <summary>
        /// Execute scalar query using SqlKata.
        /// </summary>
        public static async Task<T?> ExecuteScalarAsync<T>(this DatabaseContext db, Query query)
        {
            await db.EnsureOpenAsync();

            var compiled = db.Compiler.Compile(query);

            return await db.Connection.ExecuteScalarAsync<T>(
                compiled.Sql,
                compiled.NamedBindings,
                db.Transaction
            );
        }

        #endregion

        #region --- NHÓM 3: SQL THUẦN (Full control) ---

        /// <summary>
        /// Raw SQL query execution.
        /// Use when:
        /// - Query is complex (CTE, window functions)
        /// - Performance-critical queries
        /// - SQL is clearer than builder
        /// </summary>
        public static async Task<List<T>> SqlQueryAsync<T>(this DatabaseContext db, string sql, object? param = null)
        {
            await db.EnsureOpenAsync();
            return (await db.Connection.QueryAsync<T>(sql, param, db.Transaction)).ToList();
        }

        /// <summary>
        /// Raw SQL - return single row or default.
        /// </summary>
        public static async Task<T?> SqlFirstAsync<T>(this DatabaseContext db, string sql, object? param = null)
        {
            await db.EnsureOpenAsync();
            return await db.Connection.QueryFirstOrDefaultAsync<T>(sql, param, db.Transaction);
        }

        /// <summary>
        /// Raw SQL execution (INSERT/UPDATE/DELETE).
        /// </summary>
        public static async Task<int> SqlExecuteAsync(this DatabaseContext db, string sql, object? param = null)
        {
            await db.EnsureOpenAsync();
            return await db.Connection.ExecuteAsync(sql, param, db.Transaction);
        }

        /// <summary>
        /// Raw SQL scalar query.
        /// </summary>
        public static async Task<T?> SqlScalarAsync<T>(this DatabaseContext db, string sql, object? param = null)
        {
            await db.EnsureOpenAsync();
            return await db.Connection.ExecuteScalarAsync<T>(sql, param, db.Transaction);
        }

        #endregion
    }
}