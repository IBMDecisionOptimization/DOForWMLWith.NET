

using COM.IBM.ML.ILOG;
using ILOG.Concert;
using ILOG.CPLEX;
using System.Collections;


public class SlowMIP
{
    public static void Main()
    {
        log4net.Config.BasicConfigurator.Configure();
        log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        logger.Info("Starting the example");

        logger.Info("Credentials are " + Credentials.GetCredentials());

        try
        {
            Cplex cplex = new WmlCplex(Credentials.GetCredentials(), Runtime.DO_12_10, TShirtSize.M, 1);

            INumVar x = cplex.NumVar(0, 10);
            INumVar y = cplex.NumVar(0, 10);

            IRange c1 = (IRange)cplex.Eq(x, y);
            IRange c2 = cplex.Le(x, 4);
            IRange c3 = cplex.Ge(y, 6);

            cplex.Add(c1);
            cplex.Add(c2);
            cplex.Add(c3);

            if (!cplex.Solve())
            {
                logger.Info("Status is " + cplex.GetStatus());
                if (cplex.GetStatus().Equals(Cplex.Status.Infeasible))
                {
                    logger.Info("No Solution");
                    if (cplex.FeasOpt(new IConstraint[] { c1, c2, c3 },
                            new double[] { 1.0, 1.0, 1.0 }
                            ))
                    {
                        logger.Info("Feasopt worked.");
                        logger.Info(cplex.GetValue(x) + " " + cplex.GetValue(y));
                    }
                    else throw new Exception("Feasopt failed...");
                }
                else throw new Exception("Should not solve !!!");
            }
            else
            {
                throw new Exception("Should not solve");
            }

            cplex.End();
        }
        catch (ILOG.Concert.Exception e)
        {
            System.Console.WriteLine("Concert exception '" + e + "' caught");
        }
    }
}