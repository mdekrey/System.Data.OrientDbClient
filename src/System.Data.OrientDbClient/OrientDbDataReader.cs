using Newtonsoft.Json.Linq;
using System.Collections;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;

namespace System.Data.OrientDbClient
{
    internal class OrientDbDataReader : DbDataReader
    {
        private IEnumerator<JObject> records;
        private readonly JArray resultSet;

        public OrientDbDataReader(JArray resultSet)
        {
            this.resultSet = resultSet;
            this.records = resultSet.OfType<JObject>().GetEnumerator();
        }

        public override object this[string name] => HasProperty(name) ? Value(name) : null;

        public override object this[int ordinal] => Value(ordinal);

        public override int Depth => 0;

        public override int FieldCount => records.Current.Properties().Count();

        public override bool HasRows => resultSet.Count > 0;

        public override bool IsClosed => false;

        public override int RecordsAffected => resultSet.Count;

        public override bool GetBoolean(int ordinal) => (bool)Convert.ChangeType(Value(ordinal), typeof(bool));

        public override byte GetByte(int ordinal) => (byte)Convert.ChangeType(Value(ordinal), typeof(byte));

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException(OrientDbStrings.GetBytesNotSupported);
        }

        public override char GetChar(int ordinal) => (char)Convert.ChangeType(Value(ordinal), typeof(char));

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException(OrientDbStrings.GetCharsNotSupported);
        }

        public override string GetDataTypeName(int ordinal)
        {
            // TODO - translate from the fields given by the result json
            throw new NotImplementedException();
        }

        public override DateTime GetDateTime(int ordinal)
        {
            // TODO - parse dates
            throw new NotImplementedException();
        }

        public override decimal GetDecimal(int ordinal) => (decimal)Convert.ChangeType(Value(ordinal), typeof(decimal));

        public override double GetDouble(int ordinal) => (double)Convert.ChangeType(Value(ordinal), typeof(double));

        public override IEnumerator GetEnumerator()
        {
            return records.Current.PropertyValues().GetEnumerator();
        }

        public override Type GetFieldType(int ordinal) => Value(ordinal).GetType();

        public override float GetFloat(int ordinal) => (float)Convert.ChangeType(Value(ordinal), typeof(float));

        public override Guid GetGuid(int ordinal) => (Guid)Convert.ChangeType(Value(ordinal), typeof(Guid));

        public override short GetInt16(int ordinal) => (short)Convert.ChangeType(Value(ordinal), typeof(short));

        public override int GetInt32(int ordinal) => (int)Convert.ChangeType(Value(ordinal), typeof(int));

        public override long GetInt64(int ordinal) => (long)Convert.ChangeType(Value(ordinal), typeof(long));

        public override string GetName(int ordinal) => records.Current.Properties().ToArray()[ordinal].Name;

        public override int GetOrdinal(string name) => Array.IndexOf(records.Current.Properties().Select(p => p.Name).ToArray(), name);

        public override string GetString(int ordinal) => (string)Convert.ChangeType(Value(ordinal), typeof(string));

        public override object GetValue(int ordinal) => Value(ordinal);

        public override int GetValues(object[] values)
        {
            var originalValues = records.Current.PropertyValues().ToArray();
            var length = Math.Min(originalValues.Length, values.Length);
            Array.Copy(originalValues, values, length);
            return length;
        }

        public override bool IsDBNull(int ordinal) => Value(ordinal) is DBNull;

        public override bool NextResult()
        {
            return false;
        }

        public override bool Read()
        {
            return records.MoveNext();
        }

        private bool HasProperty(string name) => records.Current.Properties().Any(p => p.Name == name);

        private object Value(string name) => (records.Current[name] as Newtonsoft.Json.Linq.JValue)?.Value ?? NullAsDbNull(records.Current[name]);

        private object Value(int ordinal) => Value(GetName(ordinal));

        private object NullAsDbNull(JToken jToken) => (jToken is JValue) ? (((JValue)jToken).Value ?? DBNull.Value) : jToken;
    }
}