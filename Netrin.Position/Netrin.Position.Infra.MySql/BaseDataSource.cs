using Netrin.Position.Domain.Model.Auxiliar;

namespace Netrin.Position.Infra.MySql
{
    public class BaseDataSource
    {
        private readonly string _ConnectionString;
        public BaseDataSource(string connectionString)
        {
            _ConnectionString = connectionString;
        }

        protected async Task<DefaultResult<IEnumerable<T>>> Query<T>(string sql, IEnumerable<string> fields, IEnumerable<ParsedFilter> filters)
        {
            try
            {
                var connector = new DapperConnector(_ConnectionString);
                var filterHandler = new FilterHandler(filters);
                sql = sql.Replace("[fields]", string.Join(",", fields));
                if (filters.Any())
                    sql = sql.Replace("[where]", $"WHERE {filterHandler.SqlFilters}");
                else
                    sql = sql.Replace("[where]", string.Empty);
                var data = await connector.GetAsync<T>(sql, filterHandler.Parameters);

                return new DefaultResult<IEnumerable<T>>(true, System.Net.HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return new DefaultResult<IEnumerable<T>>(false, System.Net.HttpStatusCode.InternalServerError, message: $"Falha ao fazer a requisição dos dados. - {ex.Message}");
            }
        }

        protected async Task<DefaultResult<int>> InsertAsync(string sql, object parameter)
        {
            try
            {
                var connector = new DapperConnector(_ConnectionString);
                var data = await connector.InsertAsync(sql, parameter);
                return new DefaultResult<int>(true, System.Net.HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return new DefaultResult<int>(false, System.Net.HttpStatusCode.InternalServerError, message: $"Falha ao fazer a inserção dos dados. - {ex.Message}");
            }
        }

        protected async Task<DefaultResult<bool>> UpdateAsync(string sql, Dictionary<string, object> valuePairs, IEnumerable<ParsedFilter> filters)
        {
            try
            {
                if (!filters.Any())
                    return new DefaultResult<bool>(false, System.Net.HttpStatusCode.InternalServerError, message: $"Falha ao fazer a atualização dos dados, tentativa de update sem where.");

                if (!valuePairs.Any())
                    return new DefaultResult<bool>(true, System.Net.HttpStatusCode.OK, true);

                var connector = new DapperConnector(_ConnectionString);
                var filterHandler = new FilterHandler(filters);
                var updateList = new List<string>();
                var parmList = new Dapper.DynamicParameters();
                foreach (var item in valuePairs)
                {
                    updateList.Add($"{item.Key}=@p_{item.Key.Replace("`", "")}");
                    parmList.Add($"@p_{item.Key.Replace("`", "")}", item.Value);
                }
                foreach (var item in filterHandler.Parameters)
                    parmList.Add(item.Key.Replace("`", ""), item.Value);
                if (filters.Any())
                    sql = sql.Replace("[where]", $"WHERE {filterHandler.SqlFilters}");
                else
                    sql = sql.Replace("[where]", string.Empty);
                sql = sql.Replace("[fieldsandvalues]", $"SET {string.Join(", ", updateList)}");
                var data = await connector.UpdateAsync(sql, parmList);

                return new DefaultResult<bool>(true, System.Net.HttpStatusCode.OK, data);
            }
            catch (Exception ex)
            {
                return new DefaultResult<bool>(false, System.Net.HttpStatusCode.InternalServerError, message: $"Falha ao fazer a atualização dos dados. - {ex.Message}");
            }
        }
    }
}