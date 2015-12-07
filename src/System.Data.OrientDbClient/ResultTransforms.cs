using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace System.Data.OrientDbClient
{
    public static class ResultTransforms
    {
        public static DbDataReader ToReaderResult(Newtonsoft.Json.Linq.JToken orientDbResponse)
        {
            return new OrientDbDataReader(orientDbResponse["result"] as Newtonsoft.Json.Linq.JArray);
        }

        public static int ToNonQueryResult(Newtonsoft.Json.Linq.JToken result)
        {
            if (result["result"] is Newtonsoft.Json.Linq.JValue && (result["result"] as Newtonsoft.Json.Linq.JValue).Value == null)
            {
                return 0;
            }
            return (result["result"] as Newtonsoft.Json.Linq.JArray)?.Count ?? 1;
        }

        public static object ToScalarResult(Newtonsoft.Json.Linq.JToken result)
        {
            var firstRow = (result["result"] as Newtonsoft.Json.Linq.JArray).FirstOrDefault() as Newtonsoft.Json.Linq.JObject;
            if (firstRow == null)
            {
                return (result["result"] as Newtonsoft.Json.Linq.JValue)?.Value ?? DBNull.Value;
            }
            var property = firstRow.Properties().FirstOrDefault(p => !p.Name.StartsWith("@"))?.Name ?? "@rid";
            return (firstRow[property] as Newtonsoft.Json.Linq.JValue)?.Value ?? firstRow[property];
        }

    }
}
