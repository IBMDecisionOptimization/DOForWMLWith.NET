using System;
using System.Collections.Generic;
using System.IO;
using COM.IBM.ML.ILOG;
using COM.IBM.ML.ILOG.V4;
using log4net;
using Newtonsoft.Json.Linq;

namespace ILOG.CPLEX
{
    public class WmlCplex : ExternalCplex
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(WmlCplex));

        private ConnectorImpl wmlConnector;
        private String wmlName;
        private String cplexExportFormat;
        private int timeLimit;
        private JobImpl job = null;


        public WmlCplex(Credentials credentials, Runtime runtime, TShirtSize size, int numNodes) : base()
        {
            logger.Info("Starting CPLEX with " + runtime + "." + size + "." + numNodes);
            wmlConnector = (ConnectorImpl)WMLHelper.GetConnector(credentials, runtime, size, numNodes, "XML");
            wmlName = WMLHelper.GetCplexPrefix() + runtime + "." + size + "." + numNodes;

            cplexExportFormat = WMLHelper.GetSettingOrDefault("wmlconnector.v4.cplex_format", ".sav.gz");
            timeLimit = Convert.ToInt32(WMLHelper.GetSettingOrDefault("wmlconnector.v4.time_limit", "60")) * 60 * 1000;
            logger.Info("Default time limit is " + timeLimit/1000 + " minutes.");
        }


        public override void End()
        {
            logger.Info("Cleaning the CPLEX job");
            if (job != null)
            {
                logger.Info("Cleaning remaining job " + job.GetId());
                try
                {
                    wmlConnector.DeleteJob(job.GetId());
                }
                catch (Concert.Exception e)
                {
                    logger.Info("Ignoring error " + e.Message);
                }
            }
            wmlConnector.End();
            base.End();
        }


        public override Solution ExternalSolve(HashSet<String> knownVariables, HashSet<String> knownConstraints, Relaxations relaxer, Conflicts conflicts)
        {
            if (relaxer != null && conflicts != null)
                throw new Concert.Exception("Cannot run CPLEX with both relaxer and conflicts.");
            wmlConnector.InitToken();
            if (GetParam(Param.TimeLimit) == GetDefault(Param.TimeLimit))
            {
                logger.Info("Setting the time limit to default WML: " + timeLimit * 60 + " seconds");
                SetParam(Param.TimeLimit, timeLimit * 60);
            }
            else
            {
                logger.Info("Time limit has been set by user to " + GetParam(Param.TimeLimit));
            }
            string windowsTempPath = Path.GetTempPath();
            String uid = Guid.NewGuid().ToString();
            String cplexPrefix = windowsTempPath + Path.DirectorySeparatorChar + "cpx" + uid;
            String modelF = cplexPrefix + cplexExportFormat;
            String solutionF = cplexPrefix + ".sol";
            String parametersF = cplexPrefix + ".prm";
            String filtersF = cplexPrefix + ".flt";
            String mstF = cplexPrefix + ".mst";
            String annotationsF = cplexPrefix + ".ann";

            try
            {
                logger.Info("Starting export");
                long t1 = DateTime.Now.Ticks;
                ExportModel(modelF);
                long t2 = DateTime.Now.Ticks;
                logger.Info("Exported " + cplexExportFormat + " file in " + (t2 - t1) / 10000 + " milli seconds");

                WriteParam(parametersF);

                bool hasAnnotation = (GetNumDoubleAnnotations() + GetNumLongAnnotations()) != 0;

                // Filters
                if (IsMIP())
                {
                    WriteFilters(filtersF);

                    // .MST
                    try
                    {
                        GetMIPStart(0); //must be called to ensure that writeMIPStarts will not fail.
                        WriteMIPStarts(mstF);
                    }
                    catch (Exception)
                    {
                        mstF = null;
                    }
                }

                logger.Info("Exported " + cplexExportFormat + " file to " + modelF);
                logger.Info("Exported .prm file to " + parametersF);
                if (IsMIP())
                {
                        logger.Info("Exported .flt file to " + filtersF);
                        if (mstF != null) logger.Info("Exported .mst file to " + mstF);
                }
                if (hasAnnotation)
                {
                    //annotations = File.createTempFile("cpx", ".ann");
                    logger.Info("Exported " + (GetNumDoubleAnnotations() + GetNumLongAnnotations()) + " annotations to " + annotationsF);
                    WriteAnnotations(annotationsF);
                }
                else
                    logger.Info("No annotation to export.");

                try
                {
                    String deploymentId = wmlConnector.GetOrMakeDeployment(wmlName, true);

                    JArray input_data = new JArray();
                    // Its parameters
                    input_data.Add(wmlConnector.CreateDataFromFile(wmlName + ".prm", parametersF));
                    // Previous solution is any
                    if (IsMIP() && result != null && result.HasSolution()) // TODO only for MIP ?
                        input_data.Add(wmlConnector.CreateDataFromString(wmlName + ".sol", result.GetSolution()));

                    // Filters
                    if (IsMIP())
                    {
                        if (mstF != null) input_data.Add(wmlConnector.CreateDataFromFile(wmlName + ".mst", mstF));
                        input_data.Add(wmlConnector.CreateDataFromFile(wmlName + ".flt", filtersF));
                    }
                    if (hasAnnotation)
                        input_data.Add(wmlConnector.CreateDataFromFile(wmlName + ".ann", annotationsF));

                    if (relaxer != null)
                    {
                        logger.Info("Adding feasOpt support for " + relaxer.GetSize() + " elements.");
                        if (relaxer.GetSize() == 0)
                            logger.Info("Ignoring feasOpt as empty input.");
                        else
                            input_data.Add(wmlConnector.CreateDataFromBytes(wmlName + "-relaxations.feasibility", relaxer.MakeFile()));
                    }
                    if (conflicts != null)
                    {
                        logger.Info("Adding conflict support for " + conflicts.GetSize() + " elements.");
                        if (conflicts.GetSize() == 0)
                            logger.Info("Ignoring conflicts as empty input.");
                        else
                            input_data.Add(wmlConnector.CreateDataFromBytes(wmlName + "-conflicts.feasibility", conflicts.MakeFile()));
                    }

                    long t3 = DateTime.Now.Ticks;
                    byte[] payload = wmlConnector.BuildPayload(deploymentId, wmlName + cplexExportFormat, modelF, input_data, null);
                    long t4 = DateTime.Now.Ticks;
                    logger.Info("Building the payload took " + (t4 - t3) / 10000 + " milli seconds");

                    job = (JobImpl)wmlConnector.CreateAndRunEngineJob(deploymentId, payload);
                    if (job.HasSolveState())
                    {
                        logger.Info("SolveStatus = " + job.GetSolveStatus());
                    }
                    else
                    {
                        throw new Concert.Exception(job.GetJobStatus().ToString());
                    }
                    if (job.GetSolveStatus().Equals("infeasible_solution"))
                    {
                        return new Solution(CplexStatus.Infeasible.GetHashCode());
                    }
                    String sol = job.GetSolution();
                    if (sol != null)
                    {
                        // We have a feasible solution. Parse the solution file
                        //TODO useles write to disk
                        System.IO.File.WriteAllText(solutionF, sol);
                        return new Solution(sol, knownVariables, knownConstraints);
                    }
                    else
                        return new Solution(CplexStatus.Unknown.GetHashCode());
                }
                finally
                {
                    foreach (var f in new String[] { modelF, solutionF, parametersF, filtersF, mstF, annotationsF }){
                        if (f != null && File.Exists(f)) File.Delete(f);
                    }
                    if (job != null)
                    {
                        wmlConnector.DeleteJob(job.GetId());
                        job = null;
                    }
                    wmlConnector.Close();
                }
            }
            catch (System.Exception e) { 
                throw new Concert.Exception(e.Message); 
            }
        }
    }
}
