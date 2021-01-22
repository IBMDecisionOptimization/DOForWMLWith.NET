/* --------------------------------------------------------------------------
 * File: IndefQPex1.cs
 * Version 20.1.0
 * --------------------------------------------------------------------------
 * Licensed Materials - Property of IBM
 * 5725-A06 5725-A29 5724-Y48 5724-Y49 5724-Y54 5724-Y55 5655-Y21
 * Copyright IBM Corporation 2001, 2020. All Rights Reserved.
 *
 * US Government Users Restricted Rights - Use, duplication or
 * disclosure restricted by GSA ADP Schedule Contract with
 * IBM Corp.
 * --------------------------------------------------------------------------
 *
 * IndefQPex1.cs - Entering and optimizing an indefinite QP problem
 */
using COM.IBM.ML.ILOG;
using ILOG.Concert;
using ILOG.CPLEX;
using System.Collections;

public class IndefQPex1
{
    public static void Main(string[] args)
    {
        log4net.Config.BasicConfigurator.Configure();
        log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        logger.Info("Starting the example");

        logger.Info("Credentials are " + Credentials.GetCredentials());
        try
        {
            Cplex cplex = new WmlCplex(Credentials.GetCredentials(), Runtime.DO_12_10, TShirtSize.M, 1);
            ILPMatrix lp = PopulateByRow(cplex);

            int[] ind = { 0 };
            double[] val = { 1.0 };

            // When a non-convex objective function is present, CPLEX
            // will raise an exception unless the parameter
            // Cplex.Param.OptimalityTarget is set to accept
            // first-order optimal solutions
            cplex.SetParam(Cplex.Param.OptimalityTarget, 2);

            // CPLEX may converge to either local optimum 
            SolveAndDisplay(logger, cplex, lp);

            // Add a constraint that cuts off the solution at (-1, 1)
            lp.AddRow(0.0, double.MaxValue, ind, val);
            SolveAndDisplay(logger, cplex, lp);

            // Remove the newly added constraint and add a new constraint
            // with the opposite sense to cut off the solution at (1, 1)
            lp.RemoveRow(lp.Nrows - 1);
            lp.AddRow(-double.MaxValue, 0.0, ind, val);
            SolveAndDisplay(logger, cplex, lp);

            //cplex.ExportModel("indefqpex1.lp");
            cplex.End();
        }
        catch (ILOG.Concert.Exception e)
        {
            logger.Info("Concert exception '" + e + "' caught");
        }
    }

    // To populate by row, we first create the variables, and then use them to
    // create the range constraints and objective.  The model we create is:
    //
    // Minimize
    //  obj:   - 0.5 (-3 * xˆ2 - 3 * yˆ2 - 1 * x * y)
    // Subject To
    //  c1: -x + y >= 0
    //  c2:  x + y >= 0
    // Bounds
    //  -1 <= x <= 1
    //   0 <= y <= 1
    // End

    internal static ILPMatrix PopulateByRow(IMPModeler model)
    {
        ILPMatrix lp = model.AddLPMatrix();

        double[] lb = { -1.0, 0.0 };
        double[] ub = { 1.0, 1.0 };
        INumVar[] x = model.NumVarArray(model.ColumnArray(lp, 2), lb, ub);

        double[] lhs = { 0.0, 0.0 };
        double[] rhs = { double.MaxValue, double.MaxValue };
        double[][] val = {new double[] {-1.0, 1.0},
                        new double[] { 1.0, 1.0}};
        int[][] ind = {new int[] {0, 1},
                        new int[] {0, 1}};
        lp.AddRows(lhs, rhs, ind, val);

        INumExpr x00 = model.Prod(-3.0, x[0], x[0]);
        INumExpr x11 = model.Prod(-3.0, x[1], x[1]);
        INumExpr x01 = model.Prod(-1.0, x[0], x[1]);
        INumExpr Q = model.Prod(0.5, model.Sum(x00, x11, x01));

        model.Add(model.Minimize(Q));

        return (lp);
    }

    internal static void SolveAndDisplay(log4net.ILog logger, Cplex cplex, ILPMatrix lp)
    {

        if (cplex.Solve())
        {
            double[] x = cplex.GetValues(lp);
            double[] dj = cplex.GetReducedCosts(lp);
            double[] pi = cplex.GetDuals(lp);
            double[] slack = cplex.GetSlacks(lp);

            logger.Info("Solution status = " + cplex.GetStatus());
            logger.Info("Solution value  = " + cplex.GetObjValue());

            int nvars = x.Length;
            for (int j = 0; j < nvars; ++j)
            {
                logger.Info("Variable " + j + ": Value = " + x[j] + " Reduced cost = " + dj[j]);
            }

            int ncons = slack.Length;
            for (int i = 0; i < ncons; ++i)
            {
                logger.Info("Constraint " + i + ": Slack = " + slack[i] + " Pi = " + pi[i]);
            }

        }
    }
}
