// --------------------------------------------------------------------------
// File: FixCost1.cs
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
// Problem Description
// -------------------
// 
// A company must produce a product on a set of machines.
// Each machine has limited capacity.
// Producing a product on a machine has both a fixed cost
// and a cost per unit of production.
// 
// Minimize the sum of fixed and variable costs so that the
// company exactly meets demand.
// 
// --------------------------------------------------------------------------


using COM.IBM.ML.ILOG;
using ILOG.Concert;
using ILOG.CPLEX;
using System.Collections;

public class FixCost1
{
    internal static int _nbMachines = 6;
    internal static double[] _cost = { 15.0, 20.0, 45.0, 64.0, 12.0, 56.0 };
    internal static double[] _capacity = { 100.0, 20.0, 405.0, 264.0, 12.0, 256.0 };
    internal static double[] _fixedCost = { 1900.0, 820.0, 805.0, 464.0, 3912.0, 556.0 };
    internal static double _demand = 22;

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

            INumVar[] fused = cplex.BoolVarArray(_nbMachines);
            INumVar[] x = cplex.NumVarArray(_nbMachines, 0.0, double.MaxValue);

            // Objective: minimize the sum of fixed and variable costs
            cplex.AddMinimize(cplex.Sum(cplex.ScalProd(_cost, x),
                                        cplex.ScalProd(fused, _fixedCost)));

            for (int i = 0; i < _nbMachines; i++)
            {
                // Constraint: respect capacity constraint on machine 'i'
                cplex.AddLe(x[i], _capacity[i]);

                // Constraint: only produce product on machine 'i' if it is 'used'
                //             (to capture fixed cost of using machine 'i')
                cplex.AddLe(x[i], cplex.Prod(10000, fused[i]));
            }

            // Constraint: meet demand
            cplex.AddEq(cplex.Sum(x), _demand);

            if (cplex.Solve())
            {
                logger.Info("Solution status = " + cplex.GetStatus());
                logger.Info("Obj " + cplex.ObjValue);
                double eps = cplex.GetParam(Cplex.Param.MIP.Tolerances.Integrality);
                for (int i = 0; i < _nbMachines; i++)
                    if (cplex.GetValue(fused[i]) > eps)
                        logger.Info("E" + i + " is used for " +
                                           cplex.GetValue(x[i]));

                logger.Info("");
                logger.Info("----------------------------------------");
            }
            cplex.End();
        }
        catch (ILOG.Concert.Exception exc)
        {
            logger.Info("Concert exception '" + exc + "' caught");
        }
    }
}

/* Solution
Obj 1788
E5 is used for 22
*/
