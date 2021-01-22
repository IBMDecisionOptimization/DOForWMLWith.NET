using COM.IBM.ML.ILOG;
using System;

public class BrowseWML
{
    public static void Main()
    {
        log4net.Config.BasicConfigurator.Configure();
        log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        logger.Info("Starting the example");

        logger.Info("Credentials are " + Credentials.GetCredentials());
        Connector connector = null;
        try
        {
            Credentials credentials = Credentials.GetCredentials();
            connector = WMLHelper.GetConnector(credentials);
            connector.InitToken();

            logger.Info("Browsing WML");
            logger.Info(" Instances = " + connector.GetInstances().ToString());
            logger.Info(" Spaces = " + connector.GetDeploymentSpaces().ToString());
            logger.Info(" Software Specifications = " + connector.GetSoftwareSpecifications().ToString());

            logger.Info("Browsing WML by space id");
            logger.Info(" Deployments " + connector.GetDeployments().ToString());
            logger.Info(" Models " + connector.GetModels().ToString());
            logger.Info(" Jobs " + connector.GetDeploymentJobs().ToString());
            logger.Info(" Storage " + connector.GetStorage().ToString());
        }
        catch (Exception e)
        {
            logger.Info("Something bad happened " + e.Message);
        }
        finally
        {
            connector.End();
        }
    }
}