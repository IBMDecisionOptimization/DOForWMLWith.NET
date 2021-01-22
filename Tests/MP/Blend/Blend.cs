// --------------------------------------------------------------------------
// File: Blend.cs
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
// 
// Problem Description
// -------------------
// 
// Goal is to blend four sources to produce an alloy: pure metal, raw
// materials, scrap, and ingots.
// 
// Each source has a cost.
// Each source is made up of elements in different proportions.
// Alloy has minimum and maximum proportion of each element.
// 
// Minimize cost of producing a requested quantity of alloy.
// 
// --------------------------------------------------------------------------

using COM.IBM.ML.ILOG;
using ILOG.Concert;
using ILOG.CPLEX;
using System.Collections;


public class Blend
{
    internal static int _nbElements = 3;
    internal static int _nbRaw = 2;
    internal static int _nbScrap = 2;
    internal static int _nbIngot = 1;
    internal static double _alloy = 71.0;

    internal static double[] _cm = { 22.0, 10.0, 13.0 };
    internal static double[] _cr = { 6.0, 5.0 };
    internal static double[] _cs = { 7.0, 8.0 };
    internal static double[] _ci = { 9.0 };
    internal static double[] _p = { 0.05, 0.30, 0.60 };
    internal static double[] _P = { 0.10, 0.40, 0.80 };

    internal static double[][] _PRaw = {new double[] {0.20, 0.01},
                                       new double[] {0.05, 0.00},
                                       new double[] {0.05, 0.30}};
    internal static double[][] _PScrap = {new double[] {0.00, 0.01},
                                         new double[] {0.60, 0.00},
                                         new double[] {0.40, 0.70}};
    internal static double[][] _PIngot = {new double[] {0.10},
                                         new double[] {0.45},
                                         new double[] {0.45}};


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

            INumVar[] m = cplex.NumVarArray(_nbElements, 0.0, double.MaxValue);
            INumVar[] r = cplex.NumVarArray(_nbRaw, 0.0, double.MaxValue);
            INumVar[] s = cplex.NumVarArray(_nbScrap, 0.0, double.MaxValue);
            INumVar[] i = cplex.NumVarArray(_nbIngot, 0.0, double.MaxValue);
            INumVar[] e = new INumVar[_nbElements];

            // Objective Function: Minimize Cost
            cplex.AddMinimize(cplex.Sum(cplex.ScalProd(_cm, m),
                                        cplex.ScalProd(_cr, r),
                                        cplex.ScalProd(_cs, s),
                                        cplex.ScalProd(_ci, i)));

            // Min and max quantity of each element in alloy
            for (int j = 0; j < _nbElements; j++)
            {
                e[j] = cplex.NumVar(_p[j] * _alloy, _P[j] * _alloy);
            }

            // Constraint: produce requested quantity of alloy
            cplex.AddEq(cplex.Sum(e), _alloy);

            // Constraints: Satisfy element quantity requirements for alloy
            for (int j = 0; j < _nbElements; j++)
            {
                cplex.AddEq(e[j],
                            cplex.Sum(m[j],
                                      cplex.ScalProd(_PRaw[j], r),
                                      cplex.ScalProd(_PScrap[j], s),
                                      cplex.ScalProd(_PIngot[j], i)));
            }

            if (cplex.Solve())
            {
                if (cplex.GetStatus().Equals(Cplex.Status.Infeasible))
                {
                    logger.Info("No Solution");
                    return;
                }

                double[] mVals = cplex.GetValues(m);
                double[] rVals = cplex.GetValues(r);
                double[] sVals = cplex.GetValues(s);
                double[] iVals = cplex.GetValues(i);
                double[] eVals = cplex.GetValues(e);

                // Print results
                logger.Info("Solution status = " + cplex.GetStatus());
                logger.Info("Cost:" + cplex.ObjValue);

                logger.Info("Pure metal:");
                for (int j = 0; j < _nbElements; j++)
                    logger.Info("(" + j + ") " + mVals[j]);

                logger.Info("Raw material:");
                for (int j = 0; j < _nbRaw; j++)
                    logger.Info("(" + j + ") " + rVals[j]);

                logger.Info("Scrap:");
                for (int j = 0; j < _nbScrap; j++)
                    logger.Info("(" + j + ") " + sVals[j]);

                logger.Info("Ingots : ");
                for (int j = 0; j < _nbIngot; j++)
                    logger.Info("(" + j + ") " + iVals[j]);

                logger.Info("Elements:");
                for (int j = 0; j < _nbElements; j++)
                    logger.Info("(" + j + ") " + eVals[j]);
            }
            cplex.End();
        }
        catch (ILOG.Concert.Exception exc)
        {
            logger.Info("Concert exception '" + exc + "' caught");
        }
    }
}

/*
Cost:653.554
Pure metal:
0) 0
1) 0
2) 0
Raw material:
0) 0
1) 0
Scrap:
0) 17.059
1) 30.2311
Ingots : 
0) 32.4769
Elements:
0) 3.55
1) 24.85
2) 42.6
*/
