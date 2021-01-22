using COM.IBM.ML.ILOG;
using COM.IBM.ML.ILOG.V4;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;


namespace ILOG.CP
{
    public class WmlCP : ExternalCP
    {

        private readonly ILog logger = LogManager.GetLogger(typeof(WmlCP));

        /**
         * Specifies the name of the CPO command to be executed by the CPO Worker
         * (solve, refine conflict or propagate).
         */
        public static String CPO_COMMAND = "oaas.cpo.command";

        /**
         * String value for {@link #CPO_COMMAND} parameter to specify SOLVE as command
         * to be executed by CPO worker. This is the default command.
         */
        public static String CPO_COMMAND_SOLVE = "Solve";

        /**
         * String value for {@link #CPO_COMMAND} parameter to specify REFINE_CONFLICT as
         * command to be executed by CPO worker.
         */
        public static String CPO_COMMAND_REFINE_CONFLICT = "RefineConflict";

        /**
         * String value for {@link #CPO_COMMAND} parameter to specify PROPAGATE as
         * command to be executed by CPO worker.
         */
        public static String CPO_COMMAND_PROPAGATE = "Propagate";

        private String wml_name;
        private ConnectorImpl wmlConnector;
        private int timeLimit;

        private Job job;
        private String solution;

        public WmlCP(Credentials credentials, Runtime runtime, TShirtSize size, int numNodes) : base()
        {
            logger.Info("Starting CPO with " + runtime + "." + size + "." + numNodes);
            wmlConnector = (ConnectorImpl)WMLHelper.GetConnector(credentials, runtime, size, numNodes, "JSON");
            wml_name = WMLHelper.GetCPOPrefix() + runtime + "." + size + "." + numNodes;
            timeLimit = Convert.ToInt32(WMLHelper.GetSettingOrDefault("wmlconnector.v4.time_limit", "60"));
        }


        public override void End()
        {
            if (job != null)
            {
                logger.Info("Cleaning remaining job " + job.GetId());
                try
                {
                    wmlConnector.DeleteJob(job.GetId());
                }
                catch (Concert.Exception e)
                {
                    logger.Info("Ignoring error: " + e.Message);
                }
            }
            wmlConnector.End();
            base.End();
        }

        protected String ExternalProcess(String cpoCommand)
        {
            this.ResetStatus();

            wmlConnector.InitToken();
            solution = null;
            String solveStatus = null;

            if (GetParameter(DoubleParam.TimeLimit) == GetParameterDefault(DoubleParam.TimeLimit))
            {
                logger.Info("Setting the time limit to default WML: " + timeLimit * 60 + " seconds");
                SetParameter(DoubleParam.TimeLimit, timeLimit * 60);
            }
            else
            {
                logger.Info("Time limit has been set by user to " + GetParameter(DoubleParam.TimeLimit));
            }
            string windowsTempPath = Path.GetTempPath();
            String uid = Guid.NewGuid().ToString();
            String cpoPrefix = windowsTempPath + Path.DirectorySeparatorChar + "cpo" + uid;
            String modelF = cpoPrefix + ".cpo";

            try
            {
                // Create temporary files for model input and solution output.
                ExportModel(modelF);

                logger.Info("Exported cpo file.");
                try
                {
                    String deployment_id = wmlConnector.GetOrMakeDeployment(wml_name, false);
                    JArray input_data = new JArray();

                    Dictionary<String, String> overriden_solve_parameters = new Dictionary<String, String>();
                    if (cpoCommand != null)
                    {
                        overriden_solve_parameters.Add(CPO_COMMAND, cpoCommand);
                    }
                    byte[] payload = wmlConnector.BuildPayload(deployment_id, wml_name + ".cpo", modelF, input_data, overriden_solve_parameters);

                    job = wmlConnector.CreateAndRunEngineJob(deployment_id, payload);
                    if (job.HasSolveState())
                    {
                        solveStatus = job.GetSolveStatus();
                        logger.Info("SolveStatus = " + solveStatus);
                    }
                    else
                    {
                        throw new Concert.Exception(job.GetJobStatus().ToString());
                    }

                    solution = job.GetSolution();
                    return solveStatus;

                }
                finally
                {
                    if (File.Exists(modelF)) File.Delete(modelF);
                    if (job != null)
                    {
                        wmlConnector.DeleteJob(job.GetId());
                        job = null;
                    }
                    wmlConnector.Close();
                }
            }
            catch (Exception e)
            {
                throw new Concert.Exception(e.StackTrace);
            }
        }

        protected override String ExternalSolve()
        {
            return ExternalProcess(CPO_COMMAND_SOLVE);
        }


        protected override String ExternalRefineConflict()
        {
            return ExternalProcess(CPO_COMMAND_REFINE_CONFLICT);
        }

        public override WmlSolution GetSolution()
        {
            if (solution != null)
                return new WmlSolution(JObject.Parse(solution));
            else
            {
                logger.Warn("No solution");
                return new WmlSolution();
            }
        }
    }
}
