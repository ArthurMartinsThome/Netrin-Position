namespace Netrin.Position.Infra.MySql
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FilterIdentifierAttribute : System.Attribute
    {
        public readonly string FilterPropertyName;
        public FilterIdentifierAttribute(string filterPropertyName)
        {
            FilterPropertyName = filterPropertyName;
        }
    }
}