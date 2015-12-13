using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Data.OrientDbClient
{
    public class OrientDbRecordId
    {
        public short ClusterId { get; set; }
        public long ClusterPosition { get; set; }
        
        public override string ToString() =>
            $"#{ClusterId}:{ClusterPosition}";

        public static OrientDbRecordId Parse(string value) =>
            Use(value.Trim('#').Split(new[] { ':' }, 2).Select(long.Parse).ToArray());

        public Guid ToGuid() =>
            new Guid(new[] { (long)ClusterId, (long)ClusterPosition }.SelectMany(BitConverter.GetBytes).ToArray());

        public static OrientDbRecordId FromGuid(Guid value) =>
            Use(ToLongArray(value.ToByteArray()));



        private static long[] ToLongArray(byte[] v) =>
            new[] { BitConverter.ToInt64(v, 0), BitConverter.ToInt64(v, 8) };

        private static OrientDbRecordId Use(long[] parts) =>
             new OrientDbRecordId
             {
                 ClusterId = (short)parts[0],
                 ClusterPosition = parts[1],
             };
    }
}
