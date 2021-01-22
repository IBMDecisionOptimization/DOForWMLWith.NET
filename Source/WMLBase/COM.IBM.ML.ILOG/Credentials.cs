using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

namespace COM.IBM.ML.ILOG
{
    public class Credentials
    {
        public bool IsCPD;
        public Credentials(bool isCPD)
        {
            IsCPD = isCPD;
        }

        private Dictionary<String, String> credentials = new Dictionary<String, String>();
        public Credentials()
        {
        }

        public void Add(String key, String value)
        {
            credentials.Add(key, value);
        }

        public String Get(String key)
        {
            String value = null;
            if (credentials.TryGetValue(key, out value) == true)
            {
                return value;
            }
            else throw new System.Exception("Missing key" + key + " in the dictionary " + credentials);
        }

        public bool ContainsKey(String key)
        {
            return credentials.ContainsKey(key);
        }

        public static string IAM_URL = "service.iam.url";
        public static string IAM_HOST = "service.iam.host";
        public static string WML_HOST = "service.wml.host";
        public static string WML_API_KEY = "service.wml.api_key";
        public static string WML_SPACE_ID = "service.wml.space_id";
        public static string WML_VERSION = "service.wml.version";
        public static string CPD_USERNAME = "service.cpd.username";
        public static string CPD_PASSWORD = "service.cpd.password";
        public static string CPD_URL = "service.cpd.url";
        public static string PLATFORM_HOST = "service.platform.host";

        public static string COS_ENDPOINT = "service.cos.endpoint";
        public static string COS_BUCKET = "service.cos.bucket";
        public static string COS_ACCESS_KEY_ID = "service.cos.access_key_id";
        public static string COS_SECRET_ACCESS_KEY = "service.cos.secret_access_key";

        public static string[] COSFields = new String[]{
            COS_ACCESS_KEY_ID,
            COS_BUCKET,
            COS_ENDPOINT,
            COS_SECRET_ACCESS_KEY
};

        public static String[] CPDFields = new String[]{
            CPD_USERNAME,
            CPD_PASSWORD,
            WML_VERSION,
            WML_SPACE_ID,
            WML_HOST,
            CPD_URL
    };
        public static String[] publicFields = new String[]{
            IAM_URL,
            IAM_HOST,
            WML_VERSION,
            WML_SPACE_ID,
            WML_HOST,
            WML_API_KEY,
            PLATFORM_HOST
    };

        private static bool IsCPDConfig()
        {
            foreach (String key in CPDFields)
            {
                if (ConfigurationManager.AppSettings.AllKeys.Contains(key) == false)
                    return false;
            }
            return true;
        }
        private static bool IsPublicConfig()
        {
            foreach (String key in publicFields)
            {
                if (ConfigurationManager.AppSettings.AllKeys.Contains(key) == false)
                    return false;
            }
            return true;
        }

        private static bool HasCOSConfig()
        {
            foreach (String key in COSFields)
            {
                if (ConfigurationManager.AppSettings.AllKeys.Contains(key) == false)
                    return false;
            }
            return true;
        }
        public static Credentials GetCredentials()
        {
            Credentials creds = null;
            if (IsCPDConfig())
            {
                creds = GetCPDCredentials();
            }
            if (IsPublicConfig())
                creds = GetPublicCredentials();
            if (creds == null)
                throw new System.Exception("unknown config type.");
            if (HasCOSConfig())
            {
                foreach (String key in COSFields)
                {
                    creds.Add(key, WMLHelper.GetSetting(key));
                }
            }
            return creds;
        }

        private static Credentials GetCredentials(bool isCPD, String[] keys)
        {
            Credentials ret = new Credentials(isCPD);
            foreach (String val in keys)
            {
                if (ConfigurationManager.AppSettings.AllKeys.Contains(val))
                    ret.Add(val, WMLHelper.GetSetting(val));
                else
                    throw new System.Exception("Missing key " + val + " in config " + ConfigurationManager.AppSettings.AllKeys);
            }
            return ret;
        }

        private static Credentials GetCPDCredentials()
        {
            Credentials ret = GetCredentials(true, CPDFields);
            ret.Add(PLATFORM_HOST, WMLHelper.GetSetting(WML_HOST));
            return ret;
        }

        private static Credentials GetPublicCredentials()
        {
            Credentials ret = GetCredentials(false, publicFields);
            return ret;
        }
    }
}