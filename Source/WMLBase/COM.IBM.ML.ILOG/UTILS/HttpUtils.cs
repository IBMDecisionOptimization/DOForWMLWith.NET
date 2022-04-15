using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


namespace COM.IBM.ML.ILOG.UTILS
{
    public class HttpUtils : TokenHandler
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(HttpUtils));

        protected int tokenRefreshRate = Convert.ToInt32(WMLHelper.GetSettingOrDefault("wmlconnector.v4.refresh_rate", "10")) * 60 * 1000;

        protected Credentials wml_credentials;

        protected String AUTHORIZATION = "Authorization";
        private String APIKEY = "apikey";
        protected String BEARER = "bearer";
        public static String APPLICATION_JSON = "application/json";
        protected String CONTENT_TYPE = "Content-Type";
        public static String ACCEPT = "Accept";
        private String ACCESS_TOKEN = "access_token";
        private String ACCESSTOKEN = "accessToken";
        private String POST = "POST";
        private String PUT = "PUT";
        private String GET = "GET";
        private String DELETE = "DELETE";
        private String CACHE_CONTROL = "cache-control";
        private String NO_CACHE = "no-cache";
        private String VERSION = "version";
        public String SPACE_ID = "space_id";


        public static string DECISION_OPTIMIZATION = "decision_optimization";
        public static string SOLVE_STATE = "solve_state";
        public static string FAILURE = "failure";

        public static string SOLVE_STATUS = "solve_status";
        public static string STATE = "state";
        public static string STATUS = "status";
        public static string ENTITY = "entity";
        public static string DETAILS = "details";
        public static string LATEST_ENGINE_ACTIVITY = "latest_engine_activity";
        public static string OUTPUT_DATA = "output_data";
        public static string CONTENT = "content";
        public static string ID = "id";
        public static string ASSET_ID = "asset_id";
        public static string METADATA = "metadata";


        public static string RESOURCES = "resources";
        public static string NAME = "name";
        public static string LOGS = "log.txt";

        public static string MLV4 = "/ml/v4";
        public static string MLV4_DEPLOYMENT_JOBS = MLV4 + "/deployment_jobs";
        public static string MLV4_MODELS = MLV4 + "/models";
        public static string MLV4_DEPLOYMENTS = MLV4 + "/deployments";
        public static string MLV4_INSTANCES = MLV4 + "/instances";

        public static string V2 = "/v2";
        public static string V2_SOFTWARESPECS = V2 + "/software_specifications";
        public static string V2_SPACES = V2 + "/spaces";
        public static string V2_CATALOG = V2 + "/catalogs";

        public static string V2_CONNECTIONS = V2 + "/connections";


        public static string COMPLETED = "completed";
        public static string FAILED = "failed";
        public string[] status = { COMPLETED, FAILED, "canceled", "Deleted" };

        private HttpClient httpClient = null;
        protected String bearerToken;

        protected Timer timer = null;


        public static JObject ParseJson(String input)
        {
            try
            {
                return JObject.Parse(input);
            }
            catch (JsonException e)
            {
                logger.Info("Error in json should not happen!!! " + e.Message);
                return new JObject();
            }
        }
        protected String GetAuth()
        {
            if (bearerToken == null)
                logger.Info("Token is empty...");
            return BEARER + " " + bearerToken;
        }

        public HttpUtils(Credentials creds)
        {
            this.wml_credentials = creds;
        }


        public Dictionary<String, String> GetPlatformParams()
        {
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add(VERSION, wml_credentials.Get(Credentials.WML_VERSION));
            return parameters;
        }

        public Dictionary<String, String> GetWMLParams()
        {
            Dictionary<String, String> parameters = GetPlatformParams();
            parameters.Add(SPACE_ID, wml_credentials.Get(Credentials.WML_SPACE_ID));
            return parameters;
        }

        public Dictionary<String, String> GetPlatformHeaders()
        {
            Dictionary<String, String> headers = new Dictionary<String, String>();
            headers.Add(AUTHORIZATION, GetAuth());
            return headers;
        }

        public Dictionary<String, String> GetWMLHeaders()
        {
            Dictionary<String, String> headers = GetPlatformHeaders();
            headers.Add(CACHE_CONTROL, NO_CACHE);
            return headers;
        }

        public void TokenTimer(object state)
        {
            LookupToken();
        }

        public void InitToken()
        {
            if (httpClient == null)
                httpClient = new HttpClient();
            if (bearerToken != null)
                return;
            LookupToken();
            timer = new Timer(new TimerCallback(TokenTimer), null, tokenRefreshRate, tokenRefreshRate);
        }

        private void LookupToken()
        {
            if (wml_credentials.IsCPD) LookupIcpToken();
            else LookupBearerToken();
        }


        private void LookupIcpToken()
        {
            logger.Info("Lookup Bearer Token from ICP (ASYNCH)");
            Dictionary<String, String> headers = new Dictionary<String, String>();
            headers.Add(ACCEPT, APPLICATION_JSON);

            String userName = wml_credentials.Get(Credentials.CPD_USERNAME);
            String password = wml_credentials.Get(Credentials.CPD_PASSWORD);
            if (userName == null || password == null)
                throw new System.Exception("Missing credentials for CPD");

            var authString = userName + ":" + password;
            String encodedAuth = WMLHelper.EncodeBase64(authString);

            headers.Add(AUTHORIZATION, "Basic " + encodedAuth);

            Dictionary<String, String> parameters = new Dictionary<String, String>();

            String res = DoGet(
                    wml_credentials.Get(Credentials.WML_HOST),
                    wml_credentials.Get(Credentials.CPD_URL),
                            parameters,
                    headers);

            ExtractToken(res);
        }

        private void ExtractToken(String reqAnswer)
        {
            JObject json = ParseJson(reqAnswer);
            if (json.ContainsKey(ACCESS_TOKEN))
                bearerToken = json.Value<String>(ACCESS_TOKEN);
            else if (json.ContainsKey(ACCESSTOKEN))
                bearerToken = json.Value<String>(ACCESSTOKEN);
            else
                throw new System.Exception("Missing token in authentication call");
            logger.Info("Bearer Token OK");
            //logger.Info("Bearer Token OK : " + this.bearerToken);
        }

        private void LookupBearerToken()
        {
            // Cloud
            logger.Info("Lookup Bearer Token from IAM (ASYNCH)");
            Dictionary<String, String> headers = new Dictionary<String, String>();
            headers.Add(ACCEPT, APPLICATION_JSON);
            headers.Add(CONTENT_TYPE, "application/x-www-form-urlencoded");

            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters.Add("grant_type", "urn:ibm:params:oauth:grant-type:apikey");
            parameters.Add(APIKEY, wml_credentials.Get(Credentials.WML_API_KEY));

            String res = DoPost(
                    wml_credentials.Get(Credentials.IAM_HOST),
                    wml_credentials.Get(Credentials.IAM_URL),
                            parameters,
                    headers);

            ExtractToken(res);
        }


        private String BuildTargetUrl(String host, String url, Dictionary<String, String> parameters)
        {
            String targetUrl = host;
            if (targetUrl.StartsWith("https://") == false)
                targetUrl = "https://" + targetUrl;

            targetUrl = targetUrl + url;
            if (parameters != null)
            {
                if (parameters.Count != 0) targetUrl = targetUrl + "?";
                foreach (var entry in parameters)
                {
                    targetUrl = targetUrl + "&" + entry.Key + "=" + entry.Value;
                }
                if (parameters.Count != 0) targetUrl = targetUrl.Replace("?&", "?");
            }
            return targetUrl;
        }

        public void Close()
        {
            if (timer != null)
            {
                timer.Dispose();
            }
            try
            {
                if (httpClient != null) httpClient = null;
            }
            catch (IOException e)
            {
                logger.Info("Ignoring issue when closing the http client: " + e.Message);
            }
            httpClient = null;
        }
        public void End()
        {
            Close();
        }



        protected String DoCall(String host, String url, Dictionary<String, String> parameters, Dictionary<String, String> headers, byte[] body, String method)
        {
            String targetUrl = BuildTargetUrl(host, url, parameters);
            String curl = "curl --request " + method + " \"" + targetUrl + "\"";

            foreach (var it in headers)
            {
                String key = it.Key;
                String value = it.Value;
                if (key != AUTHORIZATION)
                    curl = curl + " --header \"" + key + ": " + value + "\"";
                else
                {
                    curl = curl + " --header \"" + key + ": " + BEARER + " ####\"";
                }
            }
            logger.Info("Curl info: " + curl);

            using (HttpResponseMessage getReq = GetRequest(targetUrl, method, headers, body).Result)
            {
                if (getReq.IsSuccessStatusCode)
                {
                    logger.Info("status " + getReq.StatusCode);
                    using (HttpContent content = getReq.Content)
                    {
                        return getReq.Content.ReadAsStringAsync().Result;
                    }
                }
                else
                {
                    String rawResponse = getReq.ReasonPhrase;
                    logger.Error("Error(" + getReq.StatusCode + ") calling " + url + " : " + rawResponse);
                    return null;
                }
            }
        }

        private Task<HttpResponseMessage> GetRequest(String targetUrl, String method, Dictionary<String, String> headers, byte[] body)
        {
            HttpRequestMessage request;

            if (method == GET)
            {
                request = new HttpRequestMessage(HttpMethod.Get, targetUrl);
            }
            else if (method == PUT)
            {
                request = new HttpRequestMessage(HttpMethod.Put, targetUrl);
            }
            else if (method == POST)
            {
                request = new HttpRequestMessage(HttpMethod.Post, targetUrl);
            }
            else if (method == DELETE)
            {
                request = new HttpRequestMessage(HttpMethod.Delete, targetUrl);
            }
            else throw new System.Exception("cannot call getRequest with " + method);
            if (body != null)
            {
                HttpContent content = new ByteArrayContent(body);
                request.Content = content;
            }
            else
            {
                HttpContent content = new StringContent("");
                request.Content = content;
            }
            foreach (var pair in headers)
            {
                if (pair.Key.Equals(CONTENT_TYPE))
                {
                    request.Content.Headers.Clear();
                    request.Content.Headers.Add(pair.Key, pair.Value);
                }
                else
                    request.Headers.Add(pair.Key, pair.Value);
            }
            return httpClient.SendAsync(request);
        }


        public String DoPost(String host, String targetUrl, Dictionary<String, String> parameters, Dictionary<String, String> headers)
        {
            return DoPost(host, targetUrl, parameters, headers, (String)null);
        }
        public String DoPost(String host, String targetUrl, Dictionary<String, String> parameters, Dictionary<String, String> headers, byte[] body)
        {
            return DoCall(host, targetUrl, parameters, headers, body, POST);
        }
        public String DoPost(String host, String targetUrl, Dictionary<String, String> parameters, Dictionary<String, String> headers, String body)
        {
            byte[] b = null;
            if (body != null) b = Encoding.UTF8.GetBytes(body);
            return DoPost(host, targetUrl, parameters, headers, b);
        }

        public String DoGet(String host, String targetUrl, Dictionary<String, String> parameters, Dictionary<String, String> headers)
        {
            return DoCall(host, targetUrl, parameters, headers, (byte[])null, GET);
        }

        public String DoDelete(String host, String targetUrl, Dictionary<String, String> parameters, Dictionary<String, String> headers)
        {
            return DoCall(host, targetUrl, parameters, headers, (byte[])null, DELETE);
        }

        public String DoPut(String host, String targetUrl, Dictionary<String, String> parameters, Dictionary<String, String> headers, byte[] body)
        {
            return DoCall(host, targetUrl, parameters, headers, body, PUT);
        }

        public static byte[] GetFileContent(String inputFilename)
        {
            return File.ReadAllBytes(inputFilename);
        }

        public byte[] GetFileContentAsEncoded64(String inputFile)
        {
            long t1 = DateTime.Now.Ticks;
            byte[] ret = Encoding.UTF8.GetBytes(
                WMLHelper.EncodeBase64(File.ReadAllBytes(inputFile))
                );
            long t2 = DateTime.Now.Ticks;
            logger.Info("Size of the encoded model file " + inputFile + " is " + ((ret.Length + 0.0) / 1024.0 / 1024.0) + " MB");

            logger.Info("Encoding the file " + inputFile + " took " + (t2 - t1) / 10000 + " milli seconds.");
            return ret;
        }
    }
}
