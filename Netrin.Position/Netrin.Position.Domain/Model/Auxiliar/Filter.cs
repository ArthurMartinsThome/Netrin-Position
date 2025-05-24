using Netrin.Position.Domain.Model.Enum;
using System.Collections;

namespace Netrin.Position.Domain.Model.Auxiliar
{
    public class Filter
    {
        public readonly IEnumerable<string> _Fields;
        public readonly EOperator _Operator;
        public IEnumerable<object>? _Values { get; set; }

        private Filter(EOperator @operator, IEnumerable<object> values, string fieldName)
        {
            _Fields = [fieldName];
            _Operator = @operator;
            _Values = values;
        }
        public static Filter Create<T>(string fieldName, EOperator @operator, T value)
        {
            var type = typeof(T);
            if (typeof(IEnumerable).IsAssignableFrom(typeof(T)) && type != typeof(string))
                return new Filter(@operator: @operator, ((IEnumerable)value).Cast<object>(), fieldName);
            else
                return new Filter(@operator: @operator, new object[] { value }, fieldName);
        }
    }
}