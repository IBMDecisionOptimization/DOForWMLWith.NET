using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace COM.IBM.ML.ILOG
{
    public interface Job
    {
        public void UpdateStatus();

        public String GetId();

        public JObject GetStatus();

        public String GetState();

        public bool HasSolveState();

        public bool HasFailure();
        public String GetFailure();

        public bool HasSolveStatus();

        public String GetSolveStatus();

        public JObject GetJobStatus();

        public bool HasLatestEngineActivity();

        public String GetLatestEngineActivity();

        public Dictionary<String, Object> GetKPIs();

        public JArray ExtractOutputData();

        //void delete();

        public String GetLog();

        public String GetSolution();
    }

}