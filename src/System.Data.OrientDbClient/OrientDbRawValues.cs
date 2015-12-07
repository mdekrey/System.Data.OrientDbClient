using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace System.Data.OrientDbClient
{
    public class OrientDbRawValues
    {
        protected virtual string FloatingPointFormat => "{0}E0";
        protected virtual string DateTimeFormat => @"yyyy-MM-dd HH\:mm\:ss.fffffff";
        protected virtual string DateTimeOffsetFormat => @"yyyy-MM-dd HH\:mm\:ss.fffffffzzz";
        
        public string Escape(object value)
        {
            if (value == null || value is DBNull)
            {
                return "null";
            }
            return GenerateLiteral(value);
        }
        
        public virtual string EscapeLiteral(string literal)
            => literal.Replace("\\", "\\\\").Replace("'", "\\'");


        public virtual string GenerateLiteral(object value)
        {
            return GenerateLiteralValue((dynamic)value);
        }


        protected virtual string GenerateLiteralValue(int value)
            => value.ToString();

        protected virtual string GenerateLiteralValue(short value)
            => value.ToString();

        protected virtual string GenerateLiteralValue(long value)
            => value.ToString();

        protected virtual string GenerateLiteralValue(byte value)
            => value.ToString();

        protected virtual string GenerateLiteralValue(decimal value)
            => string.Format(value.ToString(CultureInfo.InvariantCulture));

        protected virtual string GenerateLiteralValue(double value)
            => string.Format(CultureInfo.InvariantCulture, FloatingPointFormat, value);

        protected virtual string GenerateLiteralValue(float value)
            => string.Format(CultureInfo.InvariantCulture, FloatingPointFormat, value);

        protected virtual string GenerateLiteralValue(bool value)
            => value ? "1" : "0";

        protected virtual string GenerateLiteralValue(char value)
            => $"'{value}'";

        protected virtual string GenerateLiteralValue(string value)
            => $"'{EscapeLiteral(value)}'";

        protected virtual string GenerateLiteralValue(object value)
            => JsonConvert.SerializeObject(value);

        protected virtual string GenerateLiteralValue(byte[] value)
        {
            throw new NotSupportedException(OrientDbStrings.BinaryLiteralsNotSupported);
        }

        protected virtual string GenerateLiteralValue(DbType value)
        {
            throw new NotSupportedException(OrientDbStrings.DbTypeLiteralsNotSupported);
        }

        protected virtual string GenerateLiteralValue(Enum value)
            => string.Format(CultureInfo.InvariantCulture, "{0:d}", value);

        protected virtual string GenerateLiteralValue(Guid value)
            => $"'{value}'";

        protected virtual string GenerateLiteralValue(DateTime value)
            => $"'{value.ToString(DateTimeFormat, CultureInfo.InvariantCulture)}'";

        protected virtual string GenerateLiteralValue(DateTimeOffset value)
            => $"'{value.ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture)}'";

        // TODO - not the right format for TimeSpan
        protected virtual string GenerateLiteralValue(TimeSpan value)
            => $"'{value}'";
    }
}
