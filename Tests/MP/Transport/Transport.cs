

using COM.IBM.ML.ILOG;
using ILOG.Concert;
using ILOG.CPLEX;
using System.Collections;


public class Transport
{
    public static void Main(string[] args)
    {
        log4net.Config.BasicConfigurator.Configure();
        log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        logger.Info("Starting the example");

        logger.Info("Credentials are " + Credentials.GetCredentials());

        run(0, logger);
        run(1, logger);
    }
    private static void run(int type, log4net.ILog logger) {
        try
        {

            Cplex cplex = new WmlCplex(Credentials.GetCredentials(), Runtime.DO_12_10, TShirtSize.M, 1);

            int nbDemand = 4;
            int nbSupply = 3;
            double[] supply = { 1000.0, 850.0, 1250.0 };
            double[] demand = { 900.0, 1200.0, 600.0, 400.0 };

            INumVar[][] x = new INumVar[nbSupply][];
            INumVar[][] y = new INumVar[nbSupply][];

            for (int i = 0; i < nbSupply; i++)
            {
                x[i] = cplex.NumVarArray(nbDemand, 0.0, double.MaxValue);
                y[i] = cplex.NumVarArray(nbDemand, 0.0, double.MaxValue);
            }

            for (int i = 0; i < nbSupply; i++)       // supply must meet demand
                cplex.AddEq(cplex.Sum(x[i]), supply[i]);

            for (int j = 0; j < nbDemand; j++)
            {     // demand must meet supply
                ILinearNumExpr v = cplex.LinearNumExpr();
                for (int i = 0; i < nbSupply; i++)
                    v.AddTerm(1.0, x[i][j]);
                cplex.AddEq(v, demand[j]);
            }

            double[] points;
            double[] slopes;
            if (type == 0)
            {         // convex case
                points = new double[] { 200.0, 400.0 };
                slopes = new double[] { 30.0, 80.0, 130.0 };
            }
            else
            {                                  // concave case
                points = new double[] { 200.0, 400.0 };
                slopes = new double[] { 120.0, 80.0, 50.0 };
            }
            for (int i = 0; i < nbSupply; ++i)
            {
                for (int j = 0; j < nbDemand; ++j)
                {
                    cplex.AddEq(y[i][j],
                                cplex.PiecewiseLinear(x[i][j],
                                                      points, slopes, 0.0, 0.0));
                }
            }

            ILinearNumExpr expr = cplex.LinearNumExpr();
            for (int i = 0; i < nbSupply; ++i)
            {
                for (int j = 0; j < nbDemand; ++j)
                {
                    expr.AddTerm(y[i][j], 1.0);
                }
            }

            cplex.AddMinimize(expr);

            if (cplex.Solve())
            {
                logger.Info("Solution status = " + cplex.GetStatus());
                logger.Info(" - Solution: ");
                for (int i = 0; i < nbSupply; ++i)
                {
                    System.Console.Write("   " + i + ": ");
                    for (int j = 0; j < nbDemand; ++j)
                        System.Console.Write("" + cplex.GetValue(x[i][j]) + "\t");
                    logger.Info("");
                }
                logger.Info("   Cost = " + cplex.ObjValue);
            }
            cplex.End();
        }
        catch (ILOG.Concert.Exception exc)
        {
            logger.Info(exc);
        }
    }
}