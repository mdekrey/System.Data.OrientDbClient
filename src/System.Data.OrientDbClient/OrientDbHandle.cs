using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.OrientDbClient
{
    public class OrientDbHandle
    {
        private CookieContainer cookies = new CookieContainer();

        public string Database { get; set; }
        public string Password { get; set; }
        public string Server { get; set; }
        public string User { get; set; }
        public bool UseSsl { get; set; }
        public int Port { get; set; }
        public bool AttemptCreate { get; set; }

        public void ResetConnection()
        {
            cookies = new CookieContainer();
        }

        public JToken Request(string method, string command, string arguments = null, object body = null)
        {
            try
            {
                return ParseResponse((HttpWebResponse)Append(BuildRequest(method, command, arguments), body).GetResponse(), body);
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse)
            {
                return ParseResponse(ex.Response as HttpWebResponse, body);
            }
        }

        public async Task<JToken> RequestAsync(string method, string command, string arguments = null, object body = null)
        {
            try
            {
                return ParseResponse((HttpWebResponse)await (await AppendAsync(BuildRequest(method, command, arguments), body)).GetResponseAsync(), body);
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse)
            {
                return ParseResponse(ex.Response as HttpWebResponse, body);
            }
        }

        private JToken ParseResponse(HttpWebResponse response, object body)
        {
            using (response)
            {
                var success = ((int)response.StatusCode) / 100 == 2;
                if (response.ContentLength > 0)
                {
                    using (var sr = new StreamReader(response.GetResponseStream()))
                    {
                        var completeResult = Newtonsoft.Json.Linq.JObject.Parse(sr.ReadToEnd());
                        if (!success)
                        {
                            var exception = new OrientDbException(OrientDbStrings.ErrorFromOrientDb(completeResult["errors"]));
                            if (body != null)
                            {
                                exception.Data.Add("Body", JToken.FromObject(body).ToString());
                            }
                            throw exception;
                        }
                        return completeResult;
                    }
                }
                else
                {
                    if (success)
                    {
                        return new JValue((object)null);
                    }
                    else
                    {
                        var exception = new OrientDbException(OrientDbStrings.NoContentFromOrientDb);
                        if (body != null)
                        {
                            exception.Data.Add("Body", JToken.FromObject(body).ToString());
                        }
                        throw exception;
                    }
                }
            }
        }

        private HttpWebRequest Append(HttpWebRequest request, object body)
        {

            if (body != null)
            {
                if (body is string)
                {
                    var bytes = Encoding.UTF8.GetBytes(body as string);
                    request.GetRequestStream().Write(bytes, 0, bytes.Length);
                }
                else
                {
                    var bytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(body));
                    request.GetRequestStream().Write(bytes, 0, bytes.Length);
                }
            }
            return request;
        }

        private async Task<HttpWebRequest> AppendAsync(HttpWebRequest request, object body)
        {

            if (body != null)
            {
                if (body is string)
                {
                    var bytes = Encoding.UTF8.GetBytes(body as string);
                    await (await request.GetRequestStreamAsync()).WriteAsync(bytes, 0, bytes.Length);
                }
                else
                {
                    var bytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(body));
                    await (await request.GetRequestStreamAsync()).WriteAsync(bytes, 0, bytes.Length);
                }
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
                string authInfo = User + ":" + Password;
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                request.Headers["Authorization"] = "Basic " + authInfo;
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