using Netrin.Position.Domain.Model.Enum;

namespace Netrin.Position.Domain.Model.Auxiliar
{
    public class ParsedFilter
    {
        public readonly IEnumerable<string> _Fields;
        public readonly EOperator _Operator;
        public IEnumerable<object>? _Values { get; set; }

        public ParsedFilter(string fieldName, EOperator @operator, object[] values)
        {
            _Fields = [fieldName];
            _Operator = @operator;
            _Values = values;
        }
        public ParsedFilter(IEnumerable<string> fields, EOperator @operator, object[] values)
        {
            _Fields = fields;
            _Operator = @operator;
            _Values = values;
        }
    }
}