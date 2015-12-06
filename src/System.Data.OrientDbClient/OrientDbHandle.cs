using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.OrientDbClient
{
    public class OrientDbHandle
    {
        private readonly CookieContainer cookies = new CookieContainer();

        public string Database { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        public string User { get; set; }
        public bool UseSsl { get; set; }
        public int Port { get; set; }
        public bool AttemptCreate { get; set; }

        public OrientDbResponse Request(string method, string command, string arguments = null, object body = null)
        {
            try
            {
                return ParseResponse((HttpWebResponse)Append(BuildRequest(method, command, arguments), body).GetResponse());
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse)
            {
                return ParseResponse(ex.Response as HttpWebResponse);
            }
        }

        public async Task<OrientDbResponse> RequestAsync(string method, string command, string arguments = null, object body = null)
        {
            try
            {
                return ParseResponse((HttpWebResponse)await (await AppendAsync(BuildRequest(method, command, arguments), body)).GetResponseAsync());
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse)
            {
                return ParseResponse(ex.Response as HttpWebResponse);
            }
        }

        private OrientDbResponse ParseResponse(HttpWebResponse response)
        {
            using (response)
            {
                var result = new OrientDbResponse();
                result.Success = ((int)response.StatusCode) / 100 == 2;
                if (response.ContentLength > 0)
                {
                    using (var sr = new StreamReader(response.GetResponseStream()))
                    {
                        var completeResult = Newtonsoft.Json.Linq.JObject.Parse(sr.ReadToEnd());
                        if (!result.Success)
                        {
                            throw new OrientDbException(OrientDbStrings.ErrorFromOrientDb(completeResult["errors"]));
                        }
                        result.Response = completeResult["result"];
                    }
                }
                return result;
            }
        }

        private HttpWebRequest Append(HttpWebRequest request, object body)
        {

            if (body != null)
            {
                var bytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(body));
                request.GetRequestStream().Write(bytes, 0, bytes.Length);
            }
            return request;
        }

        private async Task<HttpWebRequest> AppendAsync(HttpWebRequest request, object body)
        {

            if (body != null)
            {
                var bytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(body));
                await (await request.GetRequestStreamAsync()).WriteAsync(bytes, 0, bytes.Length);
            }
            return request;
        }

        private HttpWebRequest BuildRequest(string method, string command, string arguments = null)
        {
            var url = BuildUrl(command, arguments);
            var request = WebRequest.CreateHttp(url);
            request.Method = method;
            request.CookieContainer = cookies;
            if (User != null)
            {
                request.Credentials = new NetworkCredential(User, Password);
            }

            return request;
        }

        private Uri BuildUrl(string command, string arguments)
        {
            var uriBuilder = new UriBuilder();
            uriBuilder.Scheme = UseSsl ? "https" : "http";
            uriBuilder.Host = Server;
            uriBuilder.Port = Port;
            uriBuilder.Path = $"{command}/{Database}" + (arguments == null ? "" : "/" + arguments);
            return uriBuilder.Uri;

        }
    }
}