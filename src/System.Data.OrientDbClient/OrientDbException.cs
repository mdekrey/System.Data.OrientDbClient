using System.Data.Common;
using System.Runtime.Serialization;

namespace System.Data.OrientDbClient
{
    [Serializable]
    public class OrientDbException : DbException
    {
        private object orientDbStrings;

        public OrientDbException()
        {
        }

        public OrientDbException(string message) : base(message)
        {
        }

        public OrientDbException(object orientDbStrings)
        {
            this.orientDbStrings = orientDbStrings;
        }

        public OrientDbException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OrientDbException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}