using Netrin.Position.Domain.Model.Auxiliar;
using Netrin.Position.Domain.Model.Enum;

namespace Netrin.Position.Infra.MySql
{
    public class FilterHandler
    {
        public readonly string SqlFilters;
        public readonly Dictionary<string, object> Parameters;

        public FilterHandler(IEnumerable<ParsedFilter> filters)
        {
            Parameters = new Dictionary<string, object>();

            var listFiterSql = new List<string>();
            var index = 0;
            var tempFilterSql = new List<string>();
            foreach (var item in filters)
            {
                index++;
                switch (item._Operator)
                {
                    case EOperator.Equal:
                        tempFilterSql.Add($"{item._Fields.ElementAt(0)} = @{item._Fields.ElementAt(0)}{index}");
                        Parameters.Add($"{item._Fields.ElementAt(0)}{index}", item._Values.ElementAt(0));
                        break;                        
                    case EOperator.NotEqual:
                        tempFilterSql.Add($"{item._Fields.ElementAt(0)} != @{item._Fields.ElementAt(0)}{index}");
                        Parameters.Add($"{item._Fields.ElementAt(0)}{index}", item._Values.ElementAt(0));
                        break;                        
                    default: throw new Exception("Operator not suported");
                }
            }

            listFiterSql.AddRange(tempFilterSql);            

            SqlFilters = string.Join(" AND ", listFiterSql);
        }
    }
}