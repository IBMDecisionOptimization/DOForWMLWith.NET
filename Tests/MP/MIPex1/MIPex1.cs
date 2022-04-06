// --------------------------------------------------------------------------
// File: MIPex1.cs
// Version 20.1.0
// --------------------------------------------------------------------------
// Licensed Materials - Property of IBM
// 5725-A06 5725-A29 5724-Y48 5724-Y49 5724-Y54 5724-Y55 5655-Y21
// Copyright IBM Corporation 2003, 2020. All Rights Reserved.
//
// US Government Users Restricted Rights - Use, duplication or
// disclosure restricted by GSA ADP Schedule Contract with
// IBM Corp.
// --------------------------------------------------------------------------
//
// MIPex1.cs - Entering and optimizing a MIP problem

using COM.IBM.ML.ILOG;
using ILOG.Concert;
using ILOG.CPLEX;
using System.Collections;


public class MIPex1
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
            Cplex cplex = new WmlCplex(Credentials.GetCredentials(), Runtime.DO_20_1, TShirtSize.M, 1);

            INumVar[][] var = new INumVar[1][];
            IRange[][] rng = new IRange[1][];

            PopulateByRow(cplex, var, rng);

            if (cplex.Solve())
            {
                double[] x = cplex.GetValues(var[0]);
                double[] slack = cplex.GetSlacks(rng[0]);

                logger.Info("Solution status = " + cplex.GetStatus());
                logger.Info("Solution value  = " + cplex.ObjValue);

                for (int j = 0; j < x.Length; ++j)
                {
                    logger.Info("Variable   " + j +
                                             ": Value = " + x[j]);
                }

                for (int i = 0; i < slack.Length; ++i)
                {
                    logger.Info("Constraint " + i +
                                             ": Slack = " + slack[i]);
                }
            }

            //cplex.ExportModel("mipex1.lp");
            cplex.End();
        }
        catch (ILOG.Concert.Exception e)
        {
            logger.Info("Concert exception caught '" + e + "' caught");
        }
    }


    internal static void PopulateByRow(IMPModeler model,
                                        INumVar[][] var,
                                        IRange[][] rng)
    {
        //  First define the variables, three continuous and one integer
        double[] xlb = { 0.0, 0.0, 0.0, 2.0 };
        double[] xub = {40.0, double.MaxValue,
                                   double.MaxValue, 3.0};
        NumVarType[] xt = {NumVarType.Float, NumVarType.Float,
                          NumVarType.Float, NumVarType.Int};
        INumVar[] x = model.NumVarArray(4, xlb, xub, xt);
        var[0] = x;

        // Objective Function:  maximize x0 + 2*x1 + 3*x2 + x3
        double[] objvals = { 1.0, 2.0, 3.0, 1.0 };
        model.AddMaximize(model.ScalProd(x, objvals));

        // Three constraints
        rng[0] = new IRange[3];
        // - x0 + x1 + x2 + 10*x3 <= 20
        rng[0][0] = model.AddLe(model.Sum(model.Prod(-1.0, x[0]),
                                          model.Prod(1.0, x[1]),
                                          model.Prod(1.0, x[2]),
                                          model.Prod(10.0, x[3])), 20.0);
        // x0 - 3*x1 + x2 <= 30
        rng[0][1] = model.AddLe(model.Sum(model.Prod(1.0, x[0]),
                                          model.Prod(-3.0, x[1]),
                                          model.Prod(1.0, x[2])), 30.0);
        // x1 - 3.5*x3 = 0
        rng[0][2] = model.AddEq(model.Sum(model.Prod(1.0, x[1]),
                                          model.Prod(-3.5, x[3])), 0.0);
    }
}
