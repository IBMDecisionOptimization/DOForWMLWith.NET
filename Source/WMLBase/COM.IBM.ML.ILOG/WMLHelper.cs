using System;
using System.Linq;
using System.Text;
using COM.IBM.ML.ILOG.V4;
using System.Configuration;
using log4net;

namespace COM.IBM.ML.ILOG
{
    public static class WMLHelper
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(WMLHelper));

        public static String GetTimeStamp()
        {
            return DateTime.Now.ToString().Replace(" ", "-").Replace(":", "-").Replace("/", "-");
        }
        public static String GetSettingOrDefault(String path, String defaultValue)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(path))
            {
                String value = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings[path]);
                logger.Info(String.Format("Overloading {0}: from {1} to {2}", path, defaultValue, value));
                return value;
            }
            logger.Info(String.Format("Default {0} value: {1}", path, defaultValue));
            return defaultValue;
        }
        public static String GetSetting(String path)
        {
                String value = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings[path]);
                logger.Info(String.Format("Loading setting {0}: {1}", path, value));
                return value;
        }

        public static String GetCplexPrefix()
        {
            return "CPLEXWithWML.";
        }

        public static String GetCPOPrefix()
        {
            return "CPOWithWML.";
        }

        public static String GetOPLPrefix()
        {
            return "OPLWithWML.";
        }

        public static String GetPythonPrefix()
        {
            return "PythonWithWML.";
        }

        /* Creates a connector */
        public static Connector GetConnector(Credentials creds, Runtime runtime, TShirtSize size, int nodes, String format)
        {
            return new ConnectorImpl(creds, runtime, size, nodes, format);
        }

        /* Creates a connector */
        public static Connector GetConnector(Credentials creds, Runtime runtime, TShirtSize size, int nodes)
        {
            return new ConnectorImpl(creds, runtime, size, nodes);
        }

        /* Creates a connector */
        public static Connector GetConnector(Credentials creds)
        {
            return new ConnectorImpl(creds);
        }
        public static COSConnector GetCOSConnector(Credentials creds)
        {
            return new COSConnectorImpl(creds);
        }

        public static ModelType GetCPLEXModelType(Runtime r)
        {
            switch (r)
            {
                case Runtime.DO_12_9:
                    return ModelType.CPLEX_12_9;
                case Runtime.DO_12_10:
                    return ModelType.CPLEX_12_10;
                case Runtime.DO_20_1:
                    return ModelType.CPLEX_20_1;
                case Runtime.DO_22_1:
                    return ModelType.CPLEX_22_1;
            }
            throw new System.Exception("Runtime " + r + " is not supported currently");
        }

        public static ModelType GetCPOModelType(Runtime r)
        {
            switch (r)
            {
                case Runtime.DO_12_9:
                    return ModelType.CPO_12_9;
                case Runtime.DO_12_10:
                    return ModelType.CPO_12_10;
                case Runtime.DO_20_1:
                    return ModelType.CPO_20_1;
                case Runtime.DO_22_1:
                    return ModelType.CPO_22_1;
            }
            throw new System.Exception("Runtime " + r + " is not supported currently");
        }


        public static String GetShortName(Runtime r)
        {
            switch (r)
            {
                case Runtime.DO_12_9:
                    return "do_12.9";
                case Runtime.DO_12_10:
                    return "do_12.10";
                case Runtime.DO_20_1:
                    return "do_20.1";
                case Runtime.DO_22_1:
                    return "do_22.1";
            }
            throw new System.Exception("GetShortName error " + r);
        }

        public static String GetShortName(ModelType m)
        {
            switch (m)
            {
                case ModelType.CPLEX_12_9:
                    return "do-cplex_12.9";
                case ModelType.CPO_12_9:
                    return "do-cpo_12.9";
                case ModelType.OPL_12_9:
                    return "do-opl_12.9";
                case ModelType.DOCPLEX_12_9:
                    return "do-docplex_12.9";
                case ModelType.CPLEX_12_10:
                    return "do-cplex_12.10";
                case ModelType.CPO_12_10:
                    return "do-cpo_12.10";
                case ModelType.OPL_12_10:
                    return "do-opl_12.10";
                case ModelType.DOCPLEX_12_10:
                    return "do-docplex_12.10";
                case ModelType.CPLEX_20_1:
                    return "do-cplex_20.1";
                case ModelType.CPO_20_1:
                    return "do-cpo_20.1";
                case ModelType.OPL_20_1:
                    return "do-opl_20.1";
                case ModelType.DOCPLEX_20_1:
                    return "do-docplex_20.1";
                case ModelType.CPLEX_22_1:
                    return "do-cplex_22.1";
                case ModelType.CPO_22_1:
                    return "do-cpo_22.1";
                case ModelType.OPL_22_1:
                    return "do-opl_22.1";
                case ModelType.DOCPLEX_22_1:
                    return "do-docplex_22.1";
            }
            throw new System.Exception("GetShortName error " + m);
        }
        public static string EncodeBase64(string value)
        {
            var valueBytes = Encoding.UTF8.GetBytes(value);
            return EncodeBase64(valueBytes);
        }
        public static string EncodeBase64(byte[] content)
        {
            return Convert.ToBase64String(content);
        }

        public static string DecodeBase64(string value)
        {
            var valueBytes = System.Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(valueBytes);
        }
    }
}
