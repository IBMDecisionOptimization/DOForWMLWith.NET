using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using COM.IBM.ML.ILOG.UTILS;
using System.IO;
using System.Threading;
using System.Text;

namespace COM.IBM.ML.ILOG.V4
{
    public class ConnectorImpl : HttpUtils, Connector
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(Connector));

        private Runtime wml_runtime;
        private TShirtSize wml_size;
        private int wml_nodes;
        private string resultFormat;

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


        public static string COMPLETED = "completed";
        public static string FAILED = "failed";
        private string[] status = { COMPLETED, FAILED, "canceled", "Deleted" };

        private HashSet<String> completedStatus = new HashSet<String>() { COMPLETED, FAILED, "canceled", "Deleted" };


        //TODO
        private int statusRefreshRate = Convert.ToInt32(WMLHelper.GetSettingOrDefault("wmlconnector.v4.status_rate", "500"));
        private bool showEngineProgress = Convert.ToBoolean(WMLHelper.GetSettingOrDefault("wmlconnector.v4.engine_progress", "true"));

        //TODO
        private String engineLogLevel = WMLHelper.GetSettingOrDefault("wmlconnector.v4.engine_log_level", "INFO");

        private String exportPath = null;

        //private Timer timer = null;


        public ConnectorImpl(Credentials creds) :
                this(creds, Runtime.DO_20_1, TShirtSize.M, 1, null)
        {
        }

        public ConnectorImpl(Credentials creds, Runtime runtime, TShirtSize size, int nodes)
        :
                this(creds, runtime, size, nodes, null)
        {
        }


        public ConnectorImpl(Credentials creds, Runtime runtime, TShirtSize size, int nodes, String format) : base(creds)
        {
            if (nodes == 0)
            {
                logger.Error("Cannot set a 0 node number in ");
                throw new System.Exception("Cannot set a 0 node number in ");
            }
            wml_runtime = runtime;
            wml_size = size;
            wml_nodes = nodes;
            logger.Info("Connector using V4 final APIs with runtime: " + wml_runtime + ", size: " + wml_size + ", nodes: " + wml_nodes);
            resultFormat = format;
            logger.Info("Using " + (tokenRefreshRate / 1000 / 60) + " minutes as token refresh rate");
            logger.Info("Using " + statusRefreshRate + " msec as status refresh rate");
            if (ConfigurationManager.AppSettings.AllKeys.Contains("wmlconnector.v4.export_path") == true)
            {
                String path = ConfigurationManager.AppSettings["wmlconnector.v4.export_path"];
                if (Directory.Exists(path))
                    exportPath = path;
                else
                  logger.Error("Path " + path + "does not exist. Ignoring debug export action.");
            }
            else
                logger.Info("No export path defined.");
        }



        public JObject CreateDataFromString(String id, String text)
        {
            JObject data = new JObject();
            data.Add(ID, id);

            data.Add(CONTENT, WMLHelper.EncodeBase64(text));

            return data;
        }


        public JObject CreateDataFromFile(String id, String fileName)
        {
            byte[] bytes = GetFileContent(fileName);
            return CreateDataFromBytes(id, bytes);
        }

        public JObject CreateDataFromBytes(String id, byte[] bytes)
        {
            string encoded = WMLHelper.EncodeBase64(bytes);

            JObject data = new JObject();
            data.Add(ID, id);

            data.Add(CONTENT, encoded);

            return data;
        }

        private void CopyFile(String src, String dest)
        {
            try
            {
                File.Copy(src, dest, true);
            }
            catch (IOException)
            {
                logger.Info("Copy file failed for " + src + " " + dest);
            }
        }

        private String GetPath(String export, String date, String name)
        {
            return export + "/" + date + "/" + name;
        }

        private void Dump2Disk(JArray input_data, String date)
        {
            try
            {
                for (int i = 0; i < input_data.Count; i++)
                {
                    JObject obj = input_data.Value<JObject>(i);
                    if (obj.ContainsKey(ID) && obj.ContainsKey(CONTENT))
                    {
                        String id = obj.Value<String>(ID);
                        String path = GetPath(exportPath, date, id);
                        logger.Info("Exporting " + id + " in " + path);
                        string content = WMLHelper.DecodeBase64(obj.Value<String>(CONTENT));
                        File.WriteAllText(path, content);                        
                    }
                    else
                        logger.Info("Missing " + ID + " and/or " + CONTENT);
                }
            }
            catch (IOException)
            {
                logger.Info("Ignoring IO exception when dumping info for debug.");
            }
        }

        private String GetTimeStamp()
        {
            return DateTime.Now.ToString().Replace(" ", "-").Replace(":", "-").Replace("/", "-");
        }

        public byte[] BuildPayload(String deployment_id, String sav, String savFileName, JArray input_data, Dictionary<String, String> overriden_solve_parameters)
        {
            String date = GetTimeStamp();
            if (exportPath != null)
            {
                logger.Info("Exporting the WML DO input data in " + date);
                Directory.CreateDirectory(exportPath + "/" + date);
                Dump2Disk(input_data, date);
                String path = GetPath(exportPath, date, sav);
                logger.Info("Exporting " + savFileName + " in " + path);
                CopyFile(savFileName, path);
            }

            JObject payload = new JObject();
            payload.Add(NAME, "Job_for_" + deployment_id);
            payload.Add(SPACE_ID, wml_credentials.Get(Credentials.WML_SPACE_ID));

            JObject deployment = new JObject();
            deployment.Add(ID, deployment_id);
            payload.Add("deployment", deployment);

            JObject json_do = new JObject();
            JObject solve_parameters = new JObject();
            if (showEngineProgress)
                solve_parameters.Add("oaas.logTailEnabled", "true");
            else
                solve_parameters.Add("oaas.logTailEnabled", "false");
            solve_parameters.Add("oaas.includeInputData", "false");
            solve_parameters.Add("oaas.resultsFormat", resultFormat);
            solve_parameters.Add("oaas.engineLogLevel", engineLogLevel);
            // Override default solve_parameters
            if (overriden_solve_parameters != null)
            {
                foreach (var param in overriden_solve_parameters)
                {
                    solve_parameters.Add(param.Key, param.Value);
                }
            }

            json_do.Add("solve_parameters", solve_parameters);
            payload.Add(DECISION_OPTIMIZATION, json_do);

            JArray output_data = new JArray();
            JObject outcsv = new JObject();
            outcsv.Add(ID, ".*\\.csv");
            output_data.Add(outcsv);
            JObject outtxt = new JObject();
            outtxt.Add(ID, ".*\\.txt");
            output_data.Add(outtxt);
            JObject outjson = new JObject();
            outjson.Add(ID, ".*\\.json");
            output_data.Add(outjson);
            JObject outxml = new JObject();
            outxml.Add(ID, ".*\\.xml");
            output_data.Add(outxml);
            json_do.Add(OUTPUT_DATA, output_data);

            json_do.Add("input_data", "XXX");

            String toto = payload.ToString();
            int where = toto.IndexOf("\"XXX\"");

            String before = toto.Substring(0, where);
            String after = toto.Substring(where + 5);


            String json = input_data.ToString();
            json = json.Substring(0, json.Length - 1);

            byte[] encoded = GetFileContentAsEncoded64(savFileName);

            byte[] jsid = Encoding.UTF8.GetBytes(before + json + (json.Length > 1 ? "," : "") + "{\"" + ID + "\": \"" + sav + "\"");
            byte[] jscontent = Encoding.UTF8.GetBytes(",\"" + CONTENT + "\": \"");
            byte[] jsend = Encoding.UTF8.GetBytes("\"}]" + after);

            byte[] ret = new byte[jsid.Length + jscontent.Length + jsend.Length + encoded.Length];
            Array.Copy(jsid, 0, ret, 0, jsid.Length);
            Array.Copy(jscontent, 0, ret, jsid.Length, jscontent.Length);
            Array.Copy(encoded, 0, ret, jscontent.Length + jsid.Length, encoded.Length);
            Array.Copy(jsend, 0, ret, encoded.Length + jscontent.Length + jsid.Length, jsend.Length);

            if (ret.Length > 100000000)
            {
                logger.Error("!!!! Beware: you are certainly above the WML size limits: " + ret.Length + " bytes for the model !!!");
            }
            if (exportPath != null)
            {
                String path = GetPath(exportPath, date, "/wml_payload.wml");
                logger.Info("Exporting the WML payload to " + path);
                try
                {
                    File.WriteAllText(path, Encoding.UTF8.GetString(ret));
                }
                catch (System.Exception e)
                {
                    logger.Info("Ignoring error:" + e.Message);
                }
            }
            return ret;
        }


        public Job CreateJob(String deployment_id,
                             JArray input_data,
                             JArray input_data_references,
                             JArray output_data,
                             JArray output_data_references)
        {
            logger.Info("Create job");
            JObject payload = new JObject();

            payload.Add(NAME, "Job_for_" + deployment_id);
            payload.Add(SPACE_ID, wml_credentials.Get(Credentials.WML_SPACE_ID));

            JObject deployment = new JObject();
            deployment.Add(ID, deployment_id);
            payload.Add("deployment", deployment);

            JObject json_do = new JObject();
            JObject solve_parameters = new JObject();
            solve_parameters.Add("oaas.logAttachmentName", LOGS);
            if (showEngineProgress)
                solve_parameters.Add("oaas.logTailEnabled", "true");
            else
                solve_parameters.Add("oaas.logTailEnabled", "false");
            solve_parameters.Add("oaas.includeInputData", "false");
            solve_parameters.Add("oaas.resultsFormat", resultFormat);
            solve_parameters.Add("oaas.engineLogLevel", engineLogLevel);
            json_do.Add("solve_parameters", solve_parameters);

            if (input_data != null)
                json_do.Add("input_data", input_data);

            if (input_data_references != null)
                json_do.Add("input_data_references", input_data_references);

            if (output_data != null)
                json_do.Add(OUTPUT_DATA, output_data);

            if ((output_data == null) && (output_data_references == null))
            {
                output_data = new JArray();
                JObject outcsv = new JObject();
                outcsv.Add(ID, ".*\\.csv");
                output_data.Add(outcsv);
                JObject outtxt = new JObject();
                outtxt.Add(ID, ".*\\.txt");
                output_data.Add(outtxt);
                JObject outjson = new JObject();
                outjson.Add(ID, ".*\\.json");
                output_data.Add(outjson);
                JObject outxml = new JObject();
                outxml.Add(ID, ".*\\.xml");
                output_data.Add(outxml);
                json_do.Add(OUTPUT_DATA, output_data);
            }

            if (output_data_references != null)
                json_do.Add("output_data_references", output_data_references);


            payload.Add(DECISION_OPTIMIZATION, json_do);

            Dictionary<String, String> headers = GetWMLHeaders();
            headers.Add(ACCEPT, APPLICATION_JSON);
            headers.Add(CONTENT_TYPE, APPLICATION_JSON);

            long t1 = DateTime.Now.Ticks;
            String res = DoPost(
                    wml_credentials.Get(Credentials.WML_HOST),
                    MLV4_DEPLOYMENT_JOBS,
                    GetWMLParams(), headers, payload.ToString());
            long t2 = DateTime.Now.Ticks;

            JObject json = ParseJson(res);

            String job_id = json.Value<JObject>(METADATA).Value<String>(ID);

            logger.Info("WML job_id = " + job_id);
            logger.Info("Creating the job in WML took " + (t2 - t1) / 10000 + " milli seconds.");

            return new JobImpl(this, this.wml_credentials , deployment_id, job_id);
        }

        public Job CreateEngineJob(String deployment_id,
                                   byte[] payload)
        {
            logger.Info("Create engine job");

            Dictionary<String, String> headers = GetWMLHeaders();
            headers.Add(ACCEPT, APPLICATION_JSON);
            headers.Add(CONTENT_TYPE, APPLICATION_JSON);

            long t1 = DateTime.Now.Ticks;
            String res = DoPost(
                    wml_credentials.Get(Credentials.WML_HOST),
                    MLV4_DEPLOYMENT_JOBS,
                    GetWMLParams(), headers, payload);
            //HACK
            int entityIndex = res.IndexOf("\"" + ENTITY + "\"");
            int metadataIndex = res.IndexOf("\"" + METADATA + "\"");
            if (metadataIndex > entityIndex)
            {
                res = "{" + res.Substring(metadataIndex);
            }
            else
            {
                res = res.Substring(0, entityIndex).Replace("},", "}}");
            }
            long t2 = DateTime.Now.Ticks;

            JObject json = ParseJson(res);

            String job_id = json.Value< JObject>(METADATA).Value<String>(ID);

            logger.Info("WML job_id = " + job_id);
            logger.Info("Creating the job in WML took " + (t2 - t1) / 10000 + " milli seconds.");

            return new JobImpl(this, this.wml_credentials, deployment_id, job_id);
        }

        private String WaitCompletion(Job job)
        {
            String state = null;
            do
            {
                Thread.Sleep(statusRefreshRate);

                job.UpdateStatus();

                try
                {
                    state = job.GetState();
                    if (job.HasSolveState())
                    {
                        if (job.HasSolveStatus())
                        {
                            logger.Info("WML Solve Status : " + job.GetSolveStatus());
                        }
                        if (showEngineProgress && job.HasLatestEngineActivity())
                            logger.Info("Latest Engine Activity : " + job.GetLatestEngineActivity());

                        Dictionary<String, Object> kpis = job.GetKPIs();
                        foreach (var kpi in kpis)
                        {
                            logger.Info("KPI: " + kpi.Key + " = " + kpi.Value);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    logger.Error("Error extractState: " + e);
                }

                logger.Info("Job State: " + state);
                if (state == null || state.Equals(FAILED))
                {
                    logger.Error("WML Failure: " + job.GetFailure());
                }
            } while (!completedStatus.Contains(state));
            if (state != null && state.Equals(COMPLETED))
            {
                job.ExtractOutputData();
                //logger.Info("output_data = " + output_data);
            }
            return state;
        }

        public Job CreateAndRunEngineJob(String deployment_id,
                                         byte[] input_data)
        {
            Job job = CreateEngineJob(deployment_id, input_data);

            String state = WaitCompletion(job);

            logger.Info("Job final state is " + state);
            if (exportPath != null)
            {
                String date = GetTimeStamp();
                logger.Info("Dumping the WML output in " + date);
                Directory.CreateDirectory(exportPath + "/" + date);

                String path = GetPath(exportPath, date, "/wml_answer.wml");
                logger.Info("Exporting the WML payload to " + path);
                try
                {
                    File.WriteAllText(path, job.GetStatus().ToString());
                    Dump2Disk(job.ExtractOutputData(), date);
                }
                catch (IOException e)
                {
                    logger.Info("Ignoring error: " + e.Message);
                }
            }
            return job;
        }



        public Job CreateAndRunJob(String deployment_id,
                                   JArray input_data,
                                   JArray input_data_references,
                                   JArray output_data,
                                   JArray output_data_references)
        {
            Job job = CreateJob(deployment_id, input_data, input_data_references, output_data, output_data_references);

            String state = WaitCompletion(job);

            if (state.Equals(COMPLETED))
            {
                output_data = job.ExtractOutputData();
            }
            else
            {
                logger.Error("Job is " + state);
                logger.Error("Job status:" + job.GetStatus());
            }
            return job;
        }



        public String CreateNewModel(String modelName, Runtime runtime, ModelType type, String modelAssetFilePath)
        {
            String modelId = null;
            {
                Dictionary<String, String> headers = GetWMLHeaders();
                headers.Add(CONTENT_TYPE, APPLICATION_JSON);

                JObject payload = new JObject();
                payload.Add(NAME, modelName);
                payload.Add("description", modelName);
                payload.Add("type", WMLHelper.GetShortName(type));
                JObject soft = new JObject();
                soft.Add(NAME, WMLHelper.GetShortName(runtime));
                payload.Add("software_spec", soft);
                payload.Add(SPACE_ID, wml_credentials.Get(Credentials.WML_SPACE_ID));

                String res = DoPost(
                        wml_credentials.Get(Credentials.WML_HOST),
                        MLV4_MODELS,
                        GetPlatformParams(), headers, payload.ToString());

                JObject json = ParseJson(res);
                modelId = json.Value<JObject>(METADATA).Value<String>(ID);
            }

            {
                Dictionary<String, String> headers = GetWMLHeaders();

                if (modelAssetFilePath != null)
                {
                    byte[] bytes = GetFileContent(modelAssetFilePath);

                    Dictionary<String, String> parameters = GetWMLParams();
                    parameters.Add("content_format", "native");

                    String res = DoPut(
                            wml_credentials.Get(Credentials.WML_HOST),
                            MLV4_MODELS + "/" + modelId + "/content",
                                parameters, headers, bytes);
                    if (res == null)
                        logger.Error("Problem putting the model");
                }
            }

            return modelId;
        }



        public String DeployModel(String deployName, String model_id, TShirtSize size, int nodes)
        {
            Dictionary<String, String> headers = GetWMLHeaders();
            headers.Add(CONTENT_TYPE, APPLICATION_JSON);

            JObject payload = new JObject();
            payload.Add(NAME, deployName);
            payload.Add(SPACE_ID, wml_credentials.Get(Credentials.WML_SPACE_ID));
            JObject asset = new JObject();
            asset.Add(ID, model_id);
            payload.Add("asset", asset);
            JObject hardware = new JObject();
            hardware.Add(NAME, size.ToString());
            payload.Add("hardware_spec", hardware);
            payload.Add("num_nodes", nodes);
            payload.Add("batch", new JObject());

            String res = DoPost(
                    wml_credentials.Get(Credentials.WML_HOST),
                    MLV4_DEPLOYMENTS,
                    GetPlatformParams(),
                    headers, payload.ToString());

            JObject json = ParseJson(res);
            return (json.Value<JObject>(METADATA)).Value<String>(ID);
        }

        private JObject wmlGet(String endpoint)
        {
            Dictionary<String, String> headers = GetWMLHeaders();
            headers.Add(ACCEPT, APPLICATION_JSON);

            String res = DoGet(
                        wml_credentials.Get(Credentials.WML_HOST),
                        endpoint,
                        GetWMLParams()
                        , headers);

            return ParseJson(res);
        }

        public JObject GetDeployments()
        {
            return wmlGet(MLV4_DEPLOYMENTS);
        }

        public JObject GetModels()
        {
            return wmlGet(MLV4_MODELS);
        }

        public JObject GetDeploymentJobs()
        {
            return wmlGet(MLV4_DEPLOYMENT_JOBS);
        }

        public String GetDeploymentIdByName(String deployment_name)
        {
            JObject json = GetDeployments();
            JArray resources = json.Value <JArray>(RESOURCES);
            int len = resources.Count;
            for (int i = 0; i < len; i++)
            {
                JObject metadata = resources.Value<JObject>(i).Value<JObject>(METADATA);
                if (metadata.Value<String>(NAME).Equals(deployment_name))
                {
                    return metadata.Value<String>(ID);
                }
            }
            logger.Error("Deployment " + deployment_name + " does not exist in WML");
            return null;
        }
        private void Delete(String url, Dictionary<String, String> extraParams)
        {
            Dictionary<String, String> headers = GetWMLHeaders();
            headers.Add(ACCEPT, APPLICATION_JSON);

            Dictionary<String, String> parameters = GetWMLParams();
            foreach (var p in extraParams)
                parameters.Add(p.Key, p.Value);

            DoDelete(
                    wml_credentials.Get(Credentials.WML_HOST),
                    url,
                        parameters, headers);
        }
        public void DeleteModel(String id)
        {
            Delete(MLV4_MODELS + "/" + id, new Dictionary<String, String>());
        }

        public void DeleteDeployment(String id)
        {
            Delete(MLV4_DEPLOYMENTS + "/" + id, new Dictionary<String, String>());
        }
        public void DeleteJob(String id)
        {
            String hardDelete = "true";
            bool flag = Convert.ToBoolean(WMLHelper.GetSettingOrDefault("wmlconnector.v4.hard_Delete", "false"));
            if (!flag) hardDelete = "false";

            Dictionary<String, String> hardDel = new Dictionary<String, String>();
            hardDel.Add("hard_Delete", hardDelete);
            Delete(MLV4_DEPLOYMENT_JOBS + "/" + id, hardDel);
        }

        public int DeleteModels()
        {
            JObject json = GetModels();
            JArray resources = json.Value<JArray>(RESOURCES);
            int len = resources.Count;
            for (int i = 0; i < len; i++)
            {
                JObject metadata = resources.Value< JObject>(i).Value< JObject>(METADATA);
                DeleteModel(metadata.Value<String>(ID));
            }
            logger.Info("Delete " + len + " Models");
            return len;
        }


        public int DeleteDeployments()
        {
            JObject json = GetDeployments();
            JArray resources =json.Value< JArray>(RESOURCES);
            int len = resources.Count;
            for (int i = 0; i < len; i++)
            {
                JObject metadata = resources.Value<JObject>(i).Value<JObject>(METADATA);
                DeleteDeployment(metadata.Value<String>(ID));
            }
            logger.Info("Delete " + len + " Deployments");
            return len;
        }


        public int DeleteJobs()
        {
            JObject json = GetDeploymentJobs();
            JArray resources = json.Value<JArray>(RESOURCES);
            int len = resources.Count;
            for (int i = 0; i < len; i++)
            {
                JObject metadata = resources.Value<JObject>(i).Value<JObject>(METADATA);
                DeleteJob(metadata.Value<String>(ID));
            }
            logger.Info("Delete " + len + " Jobs");
            return len;
        }


        public void CleanSpace()
        {
            logger.Info("cleanSpace called");
            int j = DeleteJobs();
            int d = DeleteDeployments();
            int m = DeleteModels();
            logger.Info("Deleted " + j + " jobs, " + d + " deployments, " + m + " models.");
        }



        public String GetOrMakeDeployment(String name, bool isCplex)
        {
            String deployment_id = this.GetDeploymentIdByName(name);
            if (deployment_id == null)
            {
                logger.Info("Creating model and deployment");
                logger.Info("Create Empty " + wml_runtime + " Model");
                ModelType type;
                if (isCplex)
                    type = WMLHelper.GetCPLEXModelType(wml_runtime);
                else
                    type = WMLHelper.GetCPOModelType(wml_runtime);

                String model_id = this.CreateNewModel(name, wml_runtime, type, null);
                logger.Info("model_id = " + model_id);

                deployment_id = this.DeployModel(name, model_id, wml_size, wml_nodes);
            }
            else
                logger.Info("Reusing deployment_id " + deployment_id);
            logger.Info("deployment_id = " + deployment_id);
            return deployment_id;
        }



        public JObject GetSoftwareSpecifications()
        {
            String res = DoGet(
                        wml_credentials.Get(Credentials.PLATFORM_HOST),
                        V2_SOFTWARESPECS,
                        GetPlatformParams(),
                        GetPlatformHeaders());
            return ParseJson(res);
        }


        public String CreateDeploymentSpace(String name, String cos_crn, String compute_name)
        {
            Dictionary<String, String> headers = GetPlatformHeaders();
            headers.Add(CONTENT_TYPE, APPLICATION_JSON);

            JObject payload = new JObject();
            payload.Add(NAME, name);
            payload.Add("description", name);
            JObject storage = new JObject();
            storage.Add("resource_crn", cos_crn);
            payload.Add("storage", storage);
            JObject crn = new JObject();
            crn.Add(NAME, compute_name);
            crn.Add("crn", cos_crn);
            JArray jarray = new JArray();
            jarray.Add(crn);
            payload.Add("compute", jarray);

            String res = DoPost(
                    wml_credentials.Get(Credentials.PLATFORM_HOST),
                    V2_SPACES,
                    GetPlatformParams(),
                    headers, payload.ToString());
            JObject json = ParseJson(res);
            return json.Value<JObject>(RESOURCES).Value<String>("ig");
        }


        public JObject GetDeploymentSpaces()
        {
            String res = DoGet(
                        wml_credentials.Get(Credentials.PLATFORM_HOST),
                        V2_SPACES,
                        GetPlatformParams(),
                        GetPlatformHeaders());
            return ParseJson(res);
        }


        public String GetDeploymentSpaceIdByName(String spaceName)
        {
            JObject json = GetDeploymentSpaces();
            JArray resources = json.Value<JArray>(RESOURCES);
            int len = resources.Count;
            for (int i = 0; i < len; i++)
            {
                JObject entity = resources.Value<JObject>(i).Value<JObject>(ENTITY);
                JObject metadata = resources.Value<JObject>(i).Value<JObject>(METADATA);
                if (entity.Value<String>(NAME).Equals(spaceName))
                    return metadata.Value<String>("id");
            }
            return null;
        }


        public JObject GetAssetFiles(String space_id)
        {
            if (wml_credentials.IsCPD)
            {
                logger.Error("GetAssetFiles is not yet implemented for CPD");
            }
            else
                logger.Error("GetAssetFiles is not yet implemented for public cloud");
            return null;
        }


        public JObject GetInstances()
        {
            if (wml_credentials.IsCPD)
            {
                logger.Error("Cannot Get WML instances in a CPD env");
                return null;
            }

            String res = DoGet(
                        wml_credentials.Get(Credentials.WML_HOST),
                        MLV4_INSTANCES,
                        GetWMLParams(), GetPlatformHeaders());

            return ParseJson(res);
        }


        public String GetCatalogIdBySpaceId(String space_id)
        {
            JObject res = ParseJson(
                DoGet(
                        wml_credentials.Get(Credentials.PLATFORM_HOST),
                        V2_CATALOG,
                        GetWMLParams(), GetPlatformHeaders()));

            if (!res.ContainsKey("catalogs"))
                return null;
            JArray catalogs = (JArray)res.Value<JArray>("catalogs");
            for (int i = 0; i < catalogs.Count; i++)
            {
                JObject catalog = (JObject)catalogs.Value<JObject>(i);
                if (catalog.Value<JObject>(ENTITY).ContainsKey(SPACE_ID) &&
                        catalog.Value<JObject>(ENTITY).Value<String>(SPACE_ID).Equals(space_id))
                    return catalog.Value<JObject>(METADATA).Value<String>("guid");
            }
            return null;
        }


        public JObject GetStorageBySpaceId(String space_id)
        {
            String res = DoGet(
                        wml_credentials.Get(Credentials.PLATFORM_HOST),
                        V2_SPACES,
                        GetWMLParams(), GetPlatformHeaders()
                );
            JArray spaces = ParseJson(res).Value<JArray>(RESOURCES);
            for (int i = 0; i < spaces.Count; i++)
            {
                JObject space = (JObject)spaces.Value< JObject>(i);
                if (space.Value<JObject>(METADATA).Value<String>("id").Equals(space_id))
                    return space.Value<JObject>(ENTITY).Value<JObject>("storage");
            }
            return null;
        }


        public JObject GetStorage()
        {
            return GetStorageBySpaceId(wml_credentials.Get(Credentials.WML_SPACE_ID));
        }


        public String GetCatalogId()
        {
            return GetCatalogIdBySpaceId(wml_credentials.Get(Credentials.WML_SPACE_ID));
        }
    }
}