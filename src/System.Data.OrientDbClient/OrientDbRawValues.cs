namespace System.Data.OrientDbClient
{
    public class OrientDbRawValues
    {
        public string Escape(object value)
        {
            if (value is string)
            {
                return GenerateLiteralValue((string)value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        protected virtual string GenerateLiteralValue(string value)
            => $"'{EscapeLiteral(value)}'";

        public virtual string EscapeLiteral(string literal)
            => literal.Replace("\\", "\\\\").Replace("'", "\\'");

    }
}