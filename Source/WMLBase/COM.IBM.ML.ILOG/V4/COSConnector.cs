using System;
using System.Collections.Generic;
using COM.IBM.ML.ILOG.UTILS;
using log4net;
using Newtonsoft.Json.Linq;

namespace COM.IBM.ML.ILOG.V4
{
    public class COSConnectorImpl : HttpUtils, COSConnector
    {
        private String _connectionId = null;
        public new void End()
        {
            if (_connectionId != null)
            {
                Dictionary<String, String> parameters = GetPlatformParams();
                Dictionary<String, String> headers = GetPlatformHeaders();

                String res = null;
                
                res = DoDelete(
                            wml_credentials.Get(Credentials.PLATFORM_HOST),
                            V2_CONNECTIONS + "/" + _connectionId,
                            parameters, headers);
                logger.Info("Deletion of Connection Id = " + _connectionId);
                _connectionId = null;
            }
            base.End();
        }
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
            String cos_bucket = wml_credentials.Get(Credentials.COS_BUCKET);
            String data = "{\n" +
                //"\"id\": \"" + id + "\",\n" +
                "\"type\": \"connection_asset\",\n" +
                "\"id\": \"" + id + "\",\n" +
                "\"connection\": {\n" +
                "\"id\": \"" + GetConnection() + "\"\n" +
                "},\n" +
                "\"location\": {\n" +
                "\"bucket\": \"" + cos_bucket + "\",\n" +
                "\"file_name\": \"" + id + "\"\n" +
                "},\n" +
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

        private String GetS3Id()
        {
            Dictionary<String, String> parameters = GetWMLParams();
            Dictionary<String, String> headers = GetPlatformHeaders();

            String res = DoGet(
                wml_credentials.Get(Credentials.PLATFORM_HOST),
                V2_COS,
                parameters, headers);
            JObject json = ParseJson(res);

            return json.Value<JObject>(METADATA).Value<String>(ASSET_ID);
    }
    public string GetConnection()
        {
            if (_connectionId != null)
                return _connectionId;
            Dictionary<String, String> parameters = GetWMLParams();
            Dictionary<String, String> headers = GetPlatformHeaders();

            headers.Add("Content-Type", "application/json");

            String cos_access_key_id = wml_credentials.Get(Credentials.COS_ACCESS_KEY_ID);
            String cos_secret_access_key = wml_credentials.Get(Credentials.COS_SECRET_ACCESS_KEY);
            String cos_origin = wml_credentials.Get(Credentials.COS_ORIGIN_COUNTRY);
            String cos_connId = GetS3Id();
            String url = wml_credentials.Get(Credentials.COS_ENDPOINT);
            String payload = "{\n" +
                    "\"name\": " + "\"s3_shared_cxn\"" + ",\n" +
                    "\"datasource_type\": \"" + cos_connId + "\",\n" +
                    "\"origin_country\": \"" + cos_origin + "\",\n" +
                    "\"properties\": {\n" +
                    "\"access_key\": \"" + cos_access_key_id + "\",\n" +
                    "\"secret_key\": \"" + cos_secret_access_key + "\",\n" +
                    "\"url\": \"" + url + "\"\n" +
                    "} \n" +
                    "}\n";

            //JObject data = ParseJson(payload);
            String t1 = WMLHelper.GetTimeStamp();
            String res = DoPost(
                    wml_credentials.Get(Credentials.PLATFORM_HOST),
                    V2_CONNECTIONS,
                    parameters, headers, payload);
            JObject json = ParseJson(res);
            String t2 = WMLHelper.GetTimeStamp();

            _connectionId = json.Value<JObject>(METADATA).Value<String>(ASSET_ID);

            logger.Info("Connection Id = " + _connectionId);
            logger.Info("Creating the connection took " + t2 + " / " + t1);
            return _connectionId;
        }
    }

}