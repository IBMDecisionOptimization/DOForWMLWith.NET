
using COM.IBM.ML.ILOG;
using ILOG.Concert;


public class CleanUpSpace
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
            connector.CleanSpace();
        }
        catch (Exception e)
        {
            logger.Info("Error: " + e.Message);
        }
        finally
        {
            connector.End();
        }

    }
}      