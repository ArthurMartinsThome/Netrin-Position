using Dapper;
using MySql.Data.MySqlClient;

namespace Netrin.Position.Infra.MySql
{
    public class DapperConnector
    {
        private readonly string _connectionString;
        public DapperConnector(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<T>> GetAsync<T>(string sql, object? parm = null)
        {
            try
            {
                var table = default(IEnumerable<T>);
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    table = await connection.QueryAsync<T>(sql, parm);
                }
                return table;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> InsertAsync(string sql, object parm)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    return await connection.ExecuteScalarAsync<int>($"{sql}; SELECT LAST_INSERT_ID();", parm);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateAsync(string sql, object parm)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    await connection.ExecuteAsync(sql, parm);
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}