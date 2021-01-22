
/* ------------------------------------------------------------

Problem Description
-------------------

The problem involves choosing colors for the countries on a map in 
such a way that at most four colors (blue, white, yellow, green) are 
used and no neighboring countries are the same color. In this exercise, 
you will find a solution for a map coloring problem with six countries: 
Belgium, Denmark, France, Germany, Luxembourg, and the Netherlands. 

------------------------------------------------------------ */

using System;
using System.IO;
using ILOG.CP;
using ILOG.Concert;
using COM.IBM.ML.ILOG;

namespace Color
{
    public class Color
    {
        public static string[] Names = { "blue", "white", "red", "green" };
        static void Main()
        {
            log4net.Config.BasicConfigurator.Configure();
            log4net.ILog logger =
                log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            Credentials credentials = new Credentials(false);

            var IAM_HOST = "https://iam.cloud.ibm.com";
            var IAM_URL = "/identity/token";
            var WML_HOST = "https://us-south.ml.cloud.ibm.com";
            var WML_API_KEY = System.Environment.GetEnvironmentVariable("WML_API_KEY");
            var WML_SPACE_ID = System.Environment.GetEnvironmentVariable("WML_SPACE_ID");
            var WML_VERSION = "2020-08-07";
            var PLATFORM_HOST = "api.dataplatform.cloud.ibm.com";

            credentials.Add(Credentials.IAM_HOST, IAM_HOST);
            credentials.Add(Credentials.IAM_URL, IAM_URL);
            credentials.Add(Credentials.WML_HOST, WML_HOST);
            credentials.Add(Credentials.WML_API_KEY, WML_API_KEY);
            credentials.Add(Credentials.WML_SPACE_ID, WML_SPACE_ID);
            credentials.Add(Credentials.WML_VERSION, WML_VERSION);
            credentials.Add(Credentials.PLATFORM_HOST, PLATFORM_HOST);

            Runtime runtime = Runtime.DO_12_10;
            TShirtSize size = TShirtSize.M;
            int numNodes = 1;

            CP cp = new WmlCP(credentials, runtime, size, numNodes);
            IIntVar Belgium = cp.IntVar(0, 3);
            IIntVar Denmark = cp.IntVar(0, 3);
            IIntVar France = cp.IntVar(0, 3);
            IIntVar Germany = cp.IntVar(0, 3);
            IIntVar Netherlands = cp.IntVar(0, 3);
            IIntVar Luxembourg = cp.IntVar(0, 3);

            cp.Add(cp.Neq(Belgium, France));
            cp.Add(cp.Neq(Belgium, Germany));
            cp.Add(cp.Neq(Belgium, Netherlands));
            cp.Add(cp.Neq(Belgium, Luxembourg));
            cp.Add(cp.Neq(Denmark, Germany));
            cp.Add(cp.Neq(France, Germany));
            cp.Add(cp.Neq(France, Luxembourg));
            cp.Add(cp.Neq(Germany, Luxembourg));
            cp.Add(cp.Neq(Germany, Netherlands));

            // Search for a solution
            if (cp.Solve())
            {
                logger.Info("Solution: ");
                logger.Info("Belgium:     " + Names[cp.GetIntValue(Belgium)]);
                logger.Info("Denmark:     " + Names[cp.GetIntValue(Denmark)]);
                logger.Info("France:      " + Names[cp.GetIntValue(France)]);
                logger.Info("Germany:     " + Names[cp.GetIntValue(Germany)]);
                logger.Info("Netherlands: " + Names[cp.GetIntValue(Netherlands)]);
                logger.Info("Luxembourg:  " + Names[cp.GetIntValue(Luxembourg)]);
            }
            else
            {
                logger.Info("Did not solve with success...");
            }
        }
    }
}

