// --------------------------------------------------------------------------
// File: FixNet.cs
// Version 20.1.0
// --------------------------------------------------------------------------
// Licensed Materials - Property of IBM
// 5725-A06 5725-A29 5724-Y48 5724-Y49 5724-Y54 5724-Y55 5655-Y21
// Copyright IBM Corporation 2018, 2020. All Rights Reserved.
//
// US Government Users Restricted Rights - Use, duplication or
// disclosure restricted by GSA ADP Schedule Contract with
// IBM Corp.
// --------------------------------------------------------------------------
//

using System;
using COM.IBM.ML.ILOG;
using ILOG.Concert;
using ILOG.CPLEX;
using System.Collections;

/// FixNet.cs - Use logical constraints to avoid numerical trouble in a
///             fixed charge network flow problem.
///
/// Find a minimum cost flow in a fixed charge network flow problem.
/// The network is as follows:
/// <pre>
///       1 -- 3 ---> demand = 1,000,000
///      / \  /       
///     /   \/        
///    0    /\        
///     \  /  \       
///      \/    \      
///       2 -- 4 ---> demand = 1
/// </pre>
/// A fixed charge of one is incurred for every edge with non-zero flow,
/// with the exception of edge <1,4>, which has a fixed charge of ten.
/// The cost per unit of flow on an edge is zero, with the exception of
/// edge <2,4>, where the cost per unit of flow is five.

public sealed class FixNet
{

    // Define origin and destination nodes for each edge, as well as
    // unit costs and fixed costs for transporting flow on each edge.
    // Note that by defining a fixed cost of 0 for each arc you just
    // get a regular min-cost flow problem.
    internal static readonly int[] orig = new int[] { 0, 0, 1, 1, 2, 2 };
    internal static readonly int[] dest = new int[] { 1, 2, 3, 4, 3, 4 };
    internal static readonly double[] unitcost = new double[] { 0, 0, 0, 0, 0, 5 };
    internal static readonly double[] fixedcost = new double[] { 1, 1, 1, 10, 1, 1 };

    // Define demand (supply) at each node.
    internal static readonly double[] demand = new double[] { -1000001, 0, 0, 1000000, 1 };

    public static void Main(string[] args)
    {
        log4net.Config.BasicConfigurator.Configure();
        log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        logger.Info("Starting the example");

        logger.Info("Credentials are " + Credentials.GetCredentials());
        Cplex cplex = new WmlCplex(Credentials.GetCredentials(), Runtime.DO_12_10, TShirtSize.M, 1);

        try
        {
            // Create the variables.
            // x variables are continuous variables in [0, infinity[,
            // f variables are binary variables.
            INumVar[] x = cplex.NumVarArray(orig.Length, 0.0, double.PositiveInfinity);
            for (int i = 0; i < x.Length; ++i)
                x[i].Name = string.Format("x{0}{1}", orig[i], dest[i]);
            INumVar[] f = cplex.BoolVarArray(orig.Length);
            for (int i = 0; i < f.Length; ++i)
                x[i].Name = string.Format("f{0}{1}", orig[i], dest[i]);

            // Create objective function.
            cplex.AddMinimize(cplex.Sum(cplex.ScalProd(unitcost, x),
                                        cplex.ScalProd(fixedcost, f)));

            // Create constraints.
            // There is one constraint for each node. The constraint for a node i
            // states that the flow leaving i and the flow entering i must differ
            // by at least the demand for i.
            for (int i = 0; i < demand.Length; ++i)
            {
                ILinearNumExpr sum = cplex.LinearNumExpr();
                for (int j = 0; j < orig.Length; ++j)
                {
                    if (orig[j] == i)
                        sum.AddTerm(-1.0, x[j]);
                    if (dest[j] == i)
                        sum.AddTerm(+1.0, x[j]);
                }
                cplex.AddGe(sum, demand[i]);
            }

            // Add logical constraints that require x[i]==0 if f[i] is 0.
            for (int i = 0; i < orig.Length; ++i)
                cplex.Add(cplex.IfThen(cplex.Eq(f[i], 0.0), cplex.Eq(x[i], 0.0)));

            // Solve the problem.
            cplex.Solve();

            // Write solution value and objective to the screen.
            Console.WriteLine("Solution status: " + cplex.GetStatus());
            Console.WriteLine("Solution value  = " + cplex.GetObjValue());
            Console.WriteLine("Solution vector:");
            foreach (INumVar v in x)
                Console.WriteLine(string.Format("{0}: {1}", v.Name, cplex.GetValue(v)));
            foreach (INumVar v in f)
                Console.WriteLine(string.Format("{0}: {1}", v.Name, cplex.GetValue(v)));

            // Finally dump the model
            cplex.ExportModel("FixNet.lp");
        }
        finally
        {
            cplex.End();
        }
    }
}
