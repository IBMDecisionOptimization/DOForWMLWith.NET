using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

using COM.IBM.ML.ILOG.UTILS;

namespace COM.IBM.ML.ILOG.V4
{
    public class JobImpl : Job
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(Job));

        String deployment_id;
        String job_id;
        JObject status = null;
        ConnectorImpl connector;
        Credentials credentials;

        public JobImpl(ConnectorImpl connector, Credentials credentials, String deployment_id, String job_id)
        {
            this.deployment_id = deployment_id;
            this.job_id = job_id;
            this.connector = connector;
            this.credentials = credentials;
        }

        public String GetId()
        {
            return job_id;
        }

        public void UpdateStatus()
        {
            try
            {
                Dictionary<String, String> headers = connector.GetWMLHeaders();
                headers.Add(HttpUtils.ACCEPT, HttpUtils.APPLICATION_JSON);

                String res = connector.DoGet(
                        credentials.Get(Credentials.WML_HOST),
                        ConnectorImpl.MLV4_DEPLOYMENT_JOBS + "/" + job_id,
                        connector.GetWMLParams(), headers);
                status = JObject.Parse(res);
            }
            catch (JsonException e)
            {
                logger.Error("Error updateStatus: " + e);
            }

        }

        public JObject GetStatus()
        {
            return status;
        }

        private JObject GetDO()
        {
            return status.Value<JObject>(ConnectorImpl.ENTITY).Value<JObject>(ConnectorImpl.DECISION_OPTIMIZATION);
        }

        private JObject GetSolveState()
        {
            return GetDO().Value<JObject>(ConnectorImpl.SOLVE_STATE);
        }

        public String GetState()
        {
            return GetDO().Value<JObject>(ConnectorImpl.STATUS).Value<String>(ConnectorImpl.STATE);
        }

        public bool HasSolveState()
        {
            return GetDO().ContainsKey(ConnectorImpl.SOLVE_STATE);
        }
        public String GetFailure()
        {
            if (HasFailure())
            {
                return GetDO().Value<JObject>(ConnectorImpl.STATUS).Value<JObject>(ConnectorImpl.FAILURE).ToString();
            }
            else return "Missing failure in WML answer.";
        }
        public bool HasFailure()
        {
            return GetDO().Value<JObject>(ConnectorImpl.STATUS).ContainsKey(ConnectorImpl.FAILURE);
        }


        public bool HasSolveStatus()
        {
            return GetSolveState().ContainsKey(ConnectorImpl.SOLVE_STATUS);
        }


        public String GetSolveStatus()
        {
            return GetSolveState().Value<String>(ConnectorImpl.SOLVE_STATUS);
        }


        public bool HasLatestEngineActivity()
        {
            return GetSolveState().ContainsKey(ConnectorImpl.LATEST_ENGINE_ACTIVITY);
        }

        public String GetLatestEngineActivity()
        {
            JArray lines = GetSolveState().Value<JArray>(ConnectorImpl.LATEST_ENGINE_ACTIVITY);
            String log = "";
            foreach (string line in lines)
                log += line + "\n";
            return log;
        }


        public Dictionary<String, Object> GetKPIs()
        {
            Dictionary<String, Object> kpis = new Dictionary<String, Object>();
            JObject details = GetSolveState().Value<JObject>(ConnectorImpl.DETAILS);
            foreach (KeyValuePair<string, JToken> pair in details)
            {
                if (pair.Key.StartsWith("KPI."))
                {
                    String kpi = pair.Key.Substring(4);
                    kpis.Add(kpi, pair.Value);
                }
            }

            return kpis;
        }


        public JArray ExtractOutputData()
        {
            try
            {
                var output_data = GetDO().Value<JArray>(ConnectorImpl.OUTPUT_DATA);
                return output_data;

            }
            catch (JsonException e)
            {
                logger.Error("Error extractSolution: " + e);
            }
            return null;
        }


        public String GetLog()
        {
            var output_data = ExtractOutputData();
            foreach (Object it in output_data)
            {
                JObject o = (JObject)it;
                if (o.Value<String>(ConnectorImpl.ID).Equals("log.txt"))
                {
                    return WMLHelper.DecodeBase64(o.Value<String>("content"));
                }
            }
            return null;
        }

        public String GetSolution()
        {
            JArray output_data = ExtractOutputData();
            String solution = null;
            foreach (Object it in output_data)
            {
                JObject o = (JObject)it;
                if (o.Value<String>(ConnectorImpl.ID).Equals("solution.json"))
                {
                    return WMLHelper.DecodeBase64(o.Value<String>("content"));
                }
                else if (o.Value<String>(ConnectorImpl.ID).Equals("solution.xml"))
                {
                    return WMLHelper.DecodeBase64(o.Value<String>("content"));
                }
                else if (o.Value<String>(ConnectorImpl.ID).EndsWith("csv"))
                {
                    solution += o.Value<String>(ConnectorImpl.ID) + "\n";
                    JArray fields = o.Value<JArray>("fields");
                    bool first = true;
                    for (int f = 0; f < fields.Count; f++)
                    {
                        if (!first)
                            solution += ",";
                        solution += fields.Value<String>(f);
                        first = false;
                    }
                    solution += "\n";
                    JArray values = o.Value<JArray>("values");
                    for (int r = 0; r < values.Count; r++)
                    {
                        JArray row = values.Value<JArray>(r);
                        first = true;
                        for (int f = 0; f < row.Count; f++)
                        {
                            if (!first)
                                solution += ",";
                            solution += row.Value<String>(f);
                            first = false;
                        }
                        solution += "\n";
                    }
                }
            }
            return solution;
        }
        public JObject GetJobStatus()
        {
            return GetDO().Value<JObject>(ConnectorImpl.STATUS);
        }
    }
}