using System;
using System.Collections.Generic;
using COM.IBM.ML.ILOG.UTILS;
using log4net;
using Newtonsoft.Json.Linq;

namespace COM.IBM.ML.ILOG.V4
{
    public class COSConnectorImpl : HttpUtils, COSConnector
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(COSConnectorImpl));

        private void CheckCredentials()
        {
            foreach (String k in Credentials.COSFields)
            {
                if (!wml_credentials.ContainsKey(k))
                {
                    throw new System.Exception("Missing config for COS: " + k);
                }
            }
        }

        public COSConnectorImpl(Credentials creds) : base(creds)
        {
            logger.Info("Creation of a COS connector.");
            if (creds.IsCPD)
            {
                logger.Info("Credentials are for CPD but deal with COS...");
            }
            CheckCredentials();
        }

        public JObject GetDataReferences(String id)
        {
            String cos_endpoint = wml_credentials.Get(Credentials.COS_ENDPOINT);
            String cos_access_key_id = wml_credentials.Get(Credentials.COS_ACCESS_KEY_ID);
            String cos_bucket = wml_credentials.Get(Credentials.COS_BUCKET);
            String cos_secret_access_key = wml_credentials.Get(Credentials.COS_SECRET_ACCESS_KEY);
            String data = "{\n" +
                    "\"id\": \"" + id + "\",\n" +
                    "\"type\": \"s3\",\n" +
                    "\"connection\": {\n" +
                    "\"endpoint_url\": \"" + cos_endpoint + "\",\n" +
                    "\"access_key_id\": \"" + cos_access_key_id + "\",\n" +
                    "\"secret_access_key\": \"" + cos_secret_access_key + "\"\n" +
                    "}, \n" +
                    "\"location\": {\n" +
                    "\"bucket\": \"" + cos_bucket + "\",\n" +
                    "\"path\": \"" + id + "\"\n" +
                    "}\n" +
                    "}\n";
            return ParseJson(data);
        }

        public String PutFile(String fileName, String filePath)
        {
            String cos_bucket = wml_credentials.Get(Credentials.COS_BUCKET);

            byte[] bytes = GetFileContent(filePath);

            Dictionary<String, String> parameters = GetPlatformParams();
            parameters.Add("content_format", "native");

            Dictionary<String, String> headers = GetPlatformHeaders();
            headers.Add("Content-Type", "text/plain");

            return DoPut(
                    wml_credentials.Get(Credentials.COS_ENDPOINT),
                    "/" + cos_bucket + "/" + fileName,
                    parameters, headers, bytes);
        }

        public String GetFile(String fileName)
        {
            String cos_bucket = wml_credentials.Get(Credentials.COS_BUCKET);

            Dictionary<String, String> parameters = GetPlatformParams();
            Dictionary<String, String> headers = GetPlatformHeaders();

            String res = DoGet(
                    wml_credentials.Get(Credentials.COS_ENDPOINT),
                    "/" + cos_bucket + "/" + fileName,
                    parameters,
                    headers);

            return res;
        }
    }

}
