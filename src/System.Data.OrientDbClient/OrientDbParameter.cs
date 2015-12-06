using System.Data.Common;

namespace System.Data.OrientDbClient
{
    public class OrientDbParameter : DbParameter
    {
        private string name;

        public override DbType DbType { get; set; }

        public override ParameterDirection Direction
        {
            get { return ParameterDirection.Input; }
            set
            {
                if (value != ParameterDirection.Input)
                {
                    throw new ArgumentException(OrientDbStrings.OnlyInputParametersSupported, nameof(value));
                }
            }
        }

        public override bool IsNullable { get; set; }

        public override string ParameterName
        {
            get { return name; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                else if (!value.StartsWith("$"))
                {
                    throw new ArgumentException(OrientDbStrings.InvalidParameterName(value), nameof(value));
                }
                name = value;
            }
        }

        public override int Size { get; set; }

        public override string SourceColumn { get; set; }

        public override bool SourceColumnNullMapping { get; set; }

        public override object Value { get; set; }

        public override void ResetDbType()
        {
            DbType = DbType.Object;
        }
    }
}