

using COM.IBM.ML.ILOG;
using ILOG.Concert;
using ILOG.CPLEX;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

//TODO
public class WMLSamples
{
    static void Main()
    {
        log4net.Config.BasicConfigurator.Configure();
        logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        logger.Info("Starting the example");
        try
        {
            LPFlow(false);
            LPFlow(true);

            FullDietPythonFlow(true);
            FullDietPythonFlow(false);

            FullWarehouseOPLFlow(false);
            FullWarehouseOPLFlow(true);
        }
        catch (System.Exception e)
        {
            logger.Info("Something bad happened: " + e.Message);
        }
    }

    private static log4net.ILog logger = null;
    private static Credentials CREDENTIALS = Credentials.GetCredentials();
    static String GetLogFromCOS(COSConnector cos)
    {
        return GetFileFromCOS(cos, "log.txt");
    }
    static String GetFileFromCOS(COSConnector cos, String fileName)
    {
        String content = cos.GetFile(fileName);
        content = content.Replace("\\r", "\n");
        return content;
    }

    static String GetLogFromJob(Job job)
    {
        return job.GetLog();
    }
    static String GetSolutionFromJob(Job job)
    {
        return job.GetSolution();
    }

    private static String CreateAndDeployDietPythonModel(Connector wml)
    {

        logger.Info("Create Python Model");
        String path = GetResourcePath() + Path.DirectorySeparatorChar + "diet.zip";

        String model_id = wml.CreateNewModel("Diet", Runtime.DO_20_1, ModelType.DOCPLEX_20_1, path);
        logger.Info("model_id = " + model_id);

        String deployment_id = wml.DeployModel("diet-test-wml-2", model_id, TShirtSize.S, 1);
        logger.Info("deployment_id = " + deployment_id);

        return deployment_id;
    }

    static void FullDietPythonFlow(bool useOutputDataReferences)
    {
        Connector wml = null;
        COSConnector cos = null;
        try
        {
            logger.Info("Full flow with Diet");

            wml = WMLHelper.GetConnector(CREDENTIALS);
            wml.InitToken();
            String deployment_id = CreateAndDeployDietPythonModel(wml);
            JArray input_data = new JArray();
            input_data.Add(CreateDataFromCSV("diet_food.csv", "diet_food.csv"));
            input_data.Add(CreateDataFromCSV("diet_food_nutrients.csv", "diet_food_nutrients.csv"));
            input_data.Add(CreateDataFromCSV("diet_nutrients.csv", "diet_nutrients.csv"));
            JArray output_data_references = null;
            if (useOutputDataReferences)
            {
                cos = WMLHelper.GetCOSConnector(CREDENTIALS);
                cos.InitToken();
                output_data_references = new JArray();
                output_data_references.Add(cos.GetDataReferences("log.txt"));
            }
            long startTime = DateTime.Now.Ticks;

            Job job = wml.CreateAndRunJob(deployment_id, input_data, null, null, output_data_references);
            if (!job.HasFailure())
            {
                if (useOutputDataReferences)
                {
                    GetLogFromCOS(cos); // Don't log
                }
                else
                {
                    logger.Info("Log:" + GetLogFromJob(job)); // Don't log
                    logger.Info("Solution:" + GetSolutionFromJob(job));
                }
                long endTime = DateTime.Now.Ticks;
                long totalTime = endTime - startTime;
                logger.Info("Total time: " + (totalTime / 1000000000.0));
                startTime = DateTime.Now.Ticks;
            }
            else
            {
                logger.Info("Error with job: " + job.GetFailure());
            }

            wml.DeleteDeployment(deployment_id);
        }
        catch (System.Exception e)
        {
            logger.Info("Error occured: " + e.Message);
        }
        finally
        {
            if (wml != null) wml.End();
            if (cos != null) cos.End();
        }
        wml.End();
    }

    static void LPFlow(bool useOutputDataReferences)
    {
        Connector wml = null;
        COSConnector cos = null;
        try
        {
            logger.Info("Full flow with LP file");

            wml = WMLHelper.GetConnector(CREDENTIALS);
            wml.InitToken();
            String deployment_id = CreateAndDeployLPModel(wml);
            JArray input_data = new JArray();
            JArray output_data_references = null;
            if (useOutputDataReferences)
            {
                cos = WMLHelper.GetCOSConnector(CREDENTIALS);
                cos.InitToken();
                output_data_references = new JArray();
                output_data_references.Add(cos.GetDataReferences("log.txt"));
            }
            long startTime = DateTime.Now.Ticks;
            Job job = wml.CreateAndRunJob(deployment_id, input_data, null, null, output_data_references);
            if (!job.HasFailure())
            {
                if (useOutputDataReferences)
                {
                    GetLogFromCOS(cos); // Don't log
                }
                else
                {
                    logger.Info("Log:" + GetLogFromJob(job)); // Don't log
                    logger.Info("Solution:" + GetSolutionFromJob(job));
                }
                long endTime = DateTime.Now.Ticks;
                long totalTime = endTime - startTime;
                logger.Info("Total time: " + (totalTime / 1000000000.0));
                startTime = DateTime.Now.Ticks;
            }
            else
            {
                logger.Info("Error with job: " + job.GetFailure());
            }
            wml.DeleteDeployment(deployment_id);
        }
        catch (System.Exception e)
        {
            logger.Info("Error occured: " + e.Message);
        }
        finally
        {
            if (wml != null) wml.End();
            if (cos != null) cos.End();
        }
        wml.End();
    }

    static String CreateAndDeployLPModel(Connector wml)
    {
        logger.Info("Create Warehouse OPL Model");
        String path = GetResourcePath() + Path.DirectorySeparatorChar + "location.lp.zip";

        String model_id = wml.CreateNewModel("location", Runtime.DO_20_1, ModelType.CPLEX_20_1, path);
        logger.Info("model_id = " + model_id);

        String deployment_id = wml.DeployModel("warehouse-lp-test-wml-2", model_id, TShirtSize.S, 1);
        logger.Info("deployment_id = " + deployment_id);

        return deployment_id;
    }
    static String CreateAndDeployWarehouseOPLModel(Connector wml)
    {
        logger.Info("Create Warehouse OPL Model");
        String path = GetResourcePath() + Path.DirectorySeparatorChar + "warehouse.zip";

        String model_id = wml.CreateNewModel("Warehouse", Runtime.DO_20_1, ModelType.OPL_20_1, path);
        logger.Info("model_id = " + model_id);

        String deployment_id = wml.DeployModel("warehouse-opl-test-wml-2", model_id, TShirtSize.S, 1);
        logger.Info("deployment_id = " + deployment_id);

        return deployment_id;
    }

    static void FullWarehouseOPLFlow(bool useOutputDataReferences)
    {
        Connector wml = null;
        COSConnector cos = null;
        try
        {
            logger.Info("Full Warehouse with OPL");

            wml = WMLHelper.GetConnector(CREDENTIALS);
            wml.InitToken();
            String deployment_id = CreateAndDeployWarehouseOPLModel(wml);

            cos = WMLHelper.GetCOSConnector(CREDENTIALS);
            cos.InitToken();
            String path = GetResourcePath() + Path.DirectorySeparatorChar + "warehouse.dat";

            if (cos.PutFile("warehouse.dat", path) != null)
            {
                JArray input_data_references = new JArray();
                input_data_references.Add(cos.GetDataReferences("warehouse.dat"));
                JArray output_data_references = null;
                if (useOutputDataReferences)
                {
                    output_data_references = new JArray();
                    output_data_references.Add(cos.GetDataReferences("log.txt"));
                }

                Job job = wml.CreateAndRunJob(deployment_id, null, input_data_references, null, output_data_references);
                if (!job.HasFailure())
                {
                    if (useOutputDataReferences)
                    {
                        logger.Info("Log:" + GetLogFromCOS(cos));
                    }
                    else
                    {
                        logger.Info("Log:" + GetLogFromJob(job));
                    }
                }
                else
                    logger.Info("Error in WML " + job.GetFailure());

                wml.DeleteDeployment(deployment_id);
            }
            else
                throw new ILOG.Concert.Exception("Error uploding in COS.");
        }
        catch (System.Exception e)
        {
            logger.Info("An error occured " + e.Message);
        }
        finally
        {
            if (wml != null) wml.End();
            if (cos != null) cos.End();
        }
        wml.End();
    }

    static String GetResourcePath()
    {
        return ".." + Path.DirectorySeparatorChar +
            ".." + Path.DirectorySeparatorChar +
            ".." + Path.DirectorySeparatorChar +
            ".." + Path.DirectorySeparatorChar +
            ".." + Path.DirectorySeparatorChar +
            ".." + Path.DirectorySeparatorChar
            + "Resources";
    }
    static String GetFileContent(String inputFilename)
    {
        String path = GetResourcePath() + Path.DirectorySeparatorChar + inputFilename;
        using (StreamReader reader = new StreamReader(path))
        {
            return reader.ReadToEnd();
        }
        //return File.ReadAllText(path, Encoding.UTF8);
    }

    static JObject CreateDataFromCSV(String id, String fileName)
    {

        JObject data = new JObject();
        data.Add("id", id);

        JArray fields = new JArray();
        JArray all_values = new JArray();
        String file = GetFileContent(fileName).Replace("\r", "");
        String[] lines = file.Split("\n");
        int nlines = lines.Length;
        String[] fields_array = Regex.Split(lines[0], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
        int nfields = fields_array.Length;
        for (int i = 0; i < nfields; i++)
        {
            String field = fields_array[i].Replace("\\", "").Replace("\"", "");
            if (field[0] == '"')
                field = field.Substring(1);
            if (field[field.Length - 1] == '"')
                field = field.Substring(0, field.Length - 1);
            fields.Add(field);
        }
        data.Add("fields", fields);

        for (int i = 1; i < nlines; i++)
        {
            JArray values = new JArray();
            String[] values_array = Regex.Split(lines[i], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            for (int j = 0; j < nfields; j++)
            {
                String value = values_array[j].Replace("\\", "").Replace("\"", ""); ;
                if (value[0] == '"')
                    value = value.Substring(1);
                if (value[value.Length - 1] == '"')
                    value = value.Substring(0, value.Length - 1);

                try
                {
                    int ivalue = int.Parse(value);
                    values.Add(ivalue);
                }
                catch (FormatException)
                {
                    try
                    {
                        double dvalue = Double.Parse(value);
                        values.Add(dvalue);
                    }
                    catch (FormatException)
                    {
                        values.Add(value);
                    }
                }
            }
            all_values.Add(values);
        }
        data.Add("values", all_values);
        return data;
    }



}