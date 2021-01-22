

using COM.IBM.ML.ILOG;
using ILOG.Concert;
using ILOG.CPLEX;
using System;
using System.Collections;
using System.Collections.Generic;

public class PrepareWML
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
                    Runtime.DO_12_10,
                    Runtime.DO_12_9
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

                            if (isCplex)
                            {
                                String name = WMLHelper.GetCplexPrefix() + runtime + "." + size + "." + node;
                                logger.Info("Looking for " + name);
                                deployments.Add(connector.GetOrMakeDeployment(name, true));
                            }
                            if (isCPO)
                            {
                                String name = WMLHelper.GetCPOPrefix() + runtime + "." + size + "." + node;
                                logger.Info("Looking for " + name);
                                deployments.Add(connector.GetOrMakeDeployment(name, false));
                            }
                            connector.End();
                        }
            logger.Info("");
            foreach (String id in deployments)
            {
                logger.Info("Deployment is " + id);
            }
        }
        catch (System.Exception e)
        {
            logger.Info("Error " + e.Message);
        }

    }
}