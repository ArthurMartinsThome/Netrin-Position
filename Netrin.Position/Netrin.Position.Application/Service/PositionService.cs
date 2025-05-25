using Netrin.Position.Domain.Interface;
using Netrin.Position.Domain.Model.Auxiliar;
using Netrin.Position.Domain.Model.Enum;
using Netrin.Position.Domain.Model.Filter;

namespace Netrin.Position.Application.Service
{
    public class PositionService
    {
        private readonly IPositionDataSource _dataSource;

        public PositionService(IPositionDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        private IEnumerable<Filter> GetFilters(PositionFilter filter)
        {
            var filterList = new List<Filter>();
            if (filter == null)
                return filterList;
            if (filter.Id.HasValue && filter.Id > 0)
                filterList.Add(Filter.Create("Id", EOperator.Equal, new[] { filter.Id.GetHashCode() }));
            if (filter.Status.HasValue && filter.Status > 0)
                filterList.Add(Filter.Create("Status", EOperator.Equal, new[] { filter.Status.GetHashCode() }));
            if (filter.HideInactive)
                filterList.Add(Filter.Create("Status", EOperator.NotEqual, new[] { EStatus.Inactive.GetHashCode(), EStatus.Deleted.GetHashCode() }));
            return filterList;
        }

        public async Task<DefaultResult<IEnumerable<Netrin.Position.Domain.Model.Position>>> Search(PositionFilter filter)
        {
            try
            {
                var result = await _dataSource.Search(GetFilters(filter));
                return result;
            }
            catch (Exception ex)
            {
                return new DefaultResult<IEnumerable<Domain.Model.Position>>(false, System.Net.HttpStatusCode.InternalServerError, message: $"Houve uma falha ao buscar a(s) posição(ões). - {ex.Message}");
            }
        }

        public async Task<DefaultResult<int>> Insert(Netrin.Position.Domain.Model.Position obj)
        {
            try
            {
                if (obj == null)
                    return new DefaultResult<int>(false, System.Net.HttpStatusCode.InternalServerError, message: $"O objeto não pode ser nulo.");
                if (string.IsNullOrEmpty(obj.Name) || obj.Latitude == null || obj.Longitude == null)
                    return new DefaultResult<int>(false, System.Net.HttpStatusCode.InternalServerError, message: $"Os campos Nome, Latitude e Longitude são obrigatórios.");

                var resultSearch = await Search(new PositionFilter()
                {
                    Status = EStatus.Active.GetHashCode()
                });

                if (!resultSearch.Succeded)
                    return new DefaultResult<int>(false, System.Net.HttpStatusCode.InternalServerError, message: $"Houve uma falha ao verificar se posição já existe.");
                if (resultSearch.Data != null)
                {
                    foreach (var position in resultSearch.Data)
                    {
                        if (position.Name == obj.Name)
                            return new DefaultResult<int>(false, System.Net.HttpStatusCode.BadRequest, message: $"Existe uma posição cadastrada e ativa com esse nome.");

                        if (position.Latitude == obj.Latitude && position.Longitude == obj.Longitude)
                            return new DefaultResult<int>(false, System.Net.HttpStatusCode.BadRequest, message: $"Já existe uma posição cadastrada e ativa com essa localização.");
                    }
                }

                obj.Status = EStatus.Active;
                var result = await _dataSource.Insert(obj);
                return result;
            }
            catch (Exception ex)
            {
                return new DefaultResult<int>(false, System.Net.HttpStatusCode.InternalServerError, message: $"Houve uma falha ao cadastrar a posição. - {ex.Message}");
            }
        }

        public async Task<DefaultResult<bool>> Update(Netrin.Position.Domain.Model.Position oldObj, Netrin.Position.Domain.Model.Position newObj)
        {
            try
            {
                if (oldObj == null || !oldObj.Id.HasValue || oldObj.Id <= 0)
                    return new DefaultResult<bool>(false, System.Net.HttpStatusCode.InternalServerError, message: $"Falta referenciar qual o id para editar a posição. - Update: oldPosition.Id == null");

                var filter = new PositionFilter() { Id = oldObj.Id };
                var filters = GetFilters(filter);
                var result = await _dataSource.Update(filters, oldObj, newObj);

                return result;
            }
            catch (Exception ex)
            {
                return new DefaultResult<bool>(false, System.Net.HttpStatusCode.InternalServerError, message: $"Houve uma falha ao editar a posição. - {ex.Message}");
            }
        }
    }
}