using Netrin.Position.Domain.Interface;
using Netrin.Position.Domain.Model.Auxiliar;
using Netrin.Position.Domain.Model.Enum;
using Netrin.Position.Infra.MySql;

namespace Netrin.Position.Adapter.MySql.DataSource
{
    public class PositionDataSource : BaseDataSource, IPositionDataSource
    {
        public PositionDataSource() : base("Server=localhost;User Id=root;Database=netrin;Password=arthur123;") { }

        #region SQL
        private const string _TableName = $"position";
        private const string _FieldId = $"id";
        private const string _FieldStatus = $"status";
        private const string _FieldName = $"name";
        private const string _FieldLatitude = $"latitude";
        private const string _FieldLongitude = $"longitude";
        private const string _FieldCreatedAt = $"created_at";
        private const string _FieldUpdatedAt = $"updated_at";

        private IEnumerable<string> _AllFields = new[]
        {
            $"{_TableName}.{_FieldId}",
            $"{_TableName}.{_FieldStatus}",
            $"{_TableName}.{_FieldName}",
            $"{_TableName}.{_FieldLatitude}",
            $"{_TableName}.{_FieldLongitude}",
            $"{_TableName}.{_FieldCreatedAt}",
            $"{_TableName}.{_FieldUpdatedAt}"
        };
        private const string SearchSql = $@"
            SELECT 
                [fields]
            FROM {_TableName}
                [where]";
        private const string InsertSql = $@"
            INSERT INTO 
                {_TableName}
                ({_FieldStatus},{_FieldName},{_FieldLatitude},{_FieldLongitude},{_FieldCreatedAt})
            VALUES
                (@{_FieldStatus},@{_FieldName},@{_FieldLatitude},@{_FieldLongitude},@{_FieldCreatedAt})";
        private const string UpdateSql = $@"
            UPDATE 
                {_TableName}
                [fieldsandvalues]
                [where]";

        #endregion

        #region Convert

        private Domain.Model.Position Convert(Model.Position obj) => new()
        {
            Id = obj.id,
            Status = (EStatus)obj.status,
            Name = obj.name,
            Latitude = obj.latitude,
            Longitude = obj.longitude,
            CreatedAt = obj.created_at,
            UpdatedAt = obj.updated_at
        };

        private Model.Position Convert(Domain.Model.Position obj) => new()
        {
            id = obj.Id,
            status = obj.Status.GetHashCode(),
            name = obj.Name,
            latitude = obj.Latitude,
            longitude = obj.Longitude,
            created_at = obj.CreatedAt,
            updated_at = obj.UpdatedAt
        };

        #endregion

        public async Task<Domain.Model.Auxiliar.DefaultResult<IEnumerable<Domain.Model.Position>>> Search(IEnumerable<Filter> filters)
        {
            try
            {
                var parsedFilters = FilterParser.Parse<MySql.Model.Position>(filters);
                var result = await Query<MySql.Model.Position>(SearchSql, _AllFields, parsedFilters);
                var returnData = result.Data.Select(Convert).ToArray();
                return new Domain.Model.Auxiliar.DefaultResult<IEnumerable<Domain.Model.Position>>(result.Succeded, result.StatusCode, returnData, result.Message);
            }
            catch (Exception ex)
            {
                return new Domain.Model.Auxiliar.DefaultResult<IEnumerable<Domain.Model.Position>>(false, System.Net.HttpStatusCode.InternalServerError, message: $"Ocorreu um erro ao buscar as posições. - {ex.Message}");
            }
        }

        public async Task<Domain.Model.Auxiliar.DefaultResult<int>> Insert(Domain.Model.Position obj)
        {
            try
            {
                obj.CreatedAt = DateTime.Now;

                var result = await InsertAsync(InsertSql, Convert(obj));
                if (!result.Succeded)
                    return new Domain.Model.Auxiliar.DefaultResult<int>(result.Succeded, result.StatusCode, message: result.Message);

                return new Domain.Model.Auxiliar.DefaultResult<int>(result.Succeded, result.StatusCode, result.Data, result.Message);
            }
            catch (Exception ex)
            {
                return new Domain.Model.Auxiliar.DefaultResult<int>(false, System.Net.HttpStatusCode.InternalServerError, message: $"Ocorreu um erro ao inserir a posição. - {ex.Message}");
            }
        }

        public async Task<Domain.Model.Auxiliar.DefaultResult<bool>> Update(IEnumerable<Filter> filters, Domain.Model.Position oldObj, Domain.Model.Position newObj)
        {
            try
            {
                var updateList = new Dictionary<string, object>();
                if (oldObj.Status != newObj.Status)
                    updateList.Add("status", newObj.Status);
                if (oldObj.Name != newObj.Name)
                    updateList.Add("name", newObj.Name);
                if (oldObj.Latitude != newObj.Latitude)
                    updateList.Add("latitude", newObj.Latitude);
                if (oldObj.Longitude != newObj.Longitude)
                    updateList.Add("longitude", newObj.Longitude);

                updateList.Add("updated_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                var parsedFilters = FilterParser.Parse<MySql.Model.Position>(filters);
                var result = await UpdateAsync(UpdateSql, updateList, parsedFilters);
                if (!result.Succeded)
                    return new Domain.Model.Auxiliar.DefaultResult<bool>(result.Succeded, result.StatusCode, message: result.Message);

                return new Domain.Model.Auxiliar.DefaultResult<bool>(result.Succeded, result.StatusCode, result.Data, result.Message);
            }
            catch (Exception ex)
            {
                return new Domain.Model.Auxiliar.DefaultResult<bool>(false, System.Net.HttpStatusCode.InternalServerError, message: $"Ocorreu um erro ao editar a posição. - {ex.Message}");
            }
        }
    }
}