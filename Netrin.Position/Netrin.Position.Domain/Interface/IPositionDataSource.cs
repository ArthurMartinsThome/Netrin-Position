using Netrin.Position.Domain.Model.Auxiliar;

namespace Netrin.Position.Domain.Interface
{
    public interface IPositionDataSource
    {
        Task<DefaultResult<IEnumerable<Netrin.Position.Domain.Model.Position>>> Search(IEnumerable<Filter> filters);
        Task<DefaultResult<int>> Insert(Netrin.Position.Domain.Model.Position obj);
        Task<DefaultResult<bool>> Update(IEnumerable<Filter> filters, Netrin.Position.Domain.Model.Position oldObj, Netrin.Position.Domain.Model.Position newObj);
    }
}