using Newtonsoft.Json.Linq;

namespace System.Data.OrientDbClient
{
    public class OrientDbResponse
    {
        public bool Success { get; set; }
        public JToken Response { get; set; }
    }
}