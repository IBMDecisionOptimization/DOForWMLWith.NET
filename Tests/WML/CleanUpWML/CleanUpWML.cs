

using COM.IBM.ML.ILOG;
using ILOG.Concert;
using ILOG.CPLEX;
using System;
using System.Collections;
using System.Collections.Generic;
using Exception = ILOG.Concert.Exception;

public class CleanUpWML
{
    public static void Main()
    {
        log4net.Config.BasicConfigurator.Configure();
        log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        logger.Info("Starting the example");
                
        try
        {
            Credentials[] credentials = new Credentials[]{
                    Credentials.GetCredentials()
            };
            Runtime[] runtimes = new Runtime[]{
                    Runtime.DO_20_1,
                    Runtime.DO_12_10
            };
            TShirtSize[] sizes = new TShirtSize[]{
                    TShirtSize.M
            };
            int[] nodes = new int[] { 1 };

            bool isCplex = true;
            bool isCPO = false;
            List<String> deployments = new List<String>();

            foreach (Credentials creds in credentials)
                foreach (Runtime runtime in runtimes)
                    foreach (TShirtSize size in sizes)
                        foreach (int node in nodes)
                        {
                            Connector connector = WMLHelper.GetConnector(
                                    creds,
                                    runtime,
                                    size,
                                    node);
                            connector.InitToken();

                            try
                            {
                                if (isCplex)
                                {
                                    String name = WMLHelper.GetCplexPrefix() + runtime + "." + size + "." + node;
                                    String id = connector.GetDeploymentIdByName(name);
                                    if (id != null)
                                    {
                                        connector.DeleteDeployment(id);
                                        deployments.Add(id);
                                    }
                                }
                                if (isCPO)
                                {
                                    String name = WMLHelper.GetCPOPrefix() + runtime + "." + size + "." + node;
                                    String id = connector.GetDeploymentIdByName(name);
                                    if (id != null)
                                    {
                                        connector.DeleteDeployment(id);
                                        deployments.Add(id);
                                    }
                                }
                            }
                            catch (Exception)
                            {

                            }
                            finally
                            {
                                connector.End();
                            }
                        }
            logger.Info("");
            foreach (String id in deployments)
            {
                logger.Info("Deployment was deleted " + id);
            }
        }
        catch (Exception e)
        {
            logger.Info("Error: " + e.Message);
        }
    }
}