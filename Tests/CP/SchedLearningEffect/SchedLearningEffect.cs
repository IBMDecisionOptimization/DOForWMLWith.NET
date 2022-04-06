// -----------------------------------------------------------------*- C# -*-
// File: ./examples/src/csharp/SchedLearningEffect.cs
// --------------------------------------------------------------------------
// Licensed Materials - Property of IBM
//
// 5724-Y48 5724-Y49 5724-Y54 5724-Y55 5725-A06 5725-A29
// Copyright IBM Corporation 1990, 2020. All Rights Reserved.
//
// Note to U.S. Government Users Restricted Rights:
// Use, duplication or disclosure restricted by GSA ADP Schedule
// Contract with IBM Corp.
// -------------------------------------------------------------------------

/* ------------------------------------------------------------

Problem Description
-------------------

This example is an extension of the classical Job-Shop Scheduling
problem (see sched_jobshop.cpp) with a learning effect on machines:
because of experience acquired by the machine, executing an
operation at position i on the machine will require less time than
if it were executed earlier at a position k < i.

More formally, each machine M_j has a learning factor alpha_j in [0,1]
such that the actual processing time of the operation executed at the
ith position on machine M_j is the decreasing function
d_j(i) = D * pow(alpha_j,i) where D is the nominal processing time of
operation.

The model for a resource, except for the classical no-overlap constraint,
consists of a chain of intervals of unknown size that forms a one-to-one
correspondance with the actual operations. The correspondance (made using
an isomorphism constraint) associates an integer variable (the position)
with each operation of the resource.  The position variable is used to
define the processing time of an operation subject to the learning effect.

This example illustrates the typical usage of the isomorphism constraint
to express relations according to the rank order of operations and to
get the position of interval variables in a sequence.

------------------------------------------------------------ */

using System;
using System.IO;
using System.Collections.Generic;
using ILOG.CP;
using ILOG.Concert;
using COM.IBM.ML.ILOG;

namespace SchedLearningEffect
{
    public class SchedLearningEffect
    {
        class DataReader
        {
            private int index = -1;
            private string[] datas;

            public DataReader(String filename)
            {
                StreamReader reader = new StreamReader(filename);
                datas = reader.ReadToEnd().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            }

            public int Next()
            {
                index++;
                return Convert.ToInt32(datas[index]);
            }

        }

        public static void Main(String[] args)
        {
            String filename = "../../../../../../Resources/cpo/learningeffect_default.data";
            int nbJobs, nbMachines;

            if (args.Length > 0)
                filename = args[0];

            log4net.Config.BasicConfigurator.Configure();
            log4net.ILog logger =
                log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            CP cp = new WmlCP(Credentials.GetCredentials(), Runtime.DO_20_1, TShirtSize.M, 1);
            DataReader data = new DataReader(filename);

            nbJobs = data.Next();
            nbMachines = data.Next();
            IIntExpr[] ends = new IIntExpr[nbJobs];

            IIntervalVar[][] machines = new IIntervalVar[nbMachines][];
            int[][] sizes = new int[nbMachines][];
            for (int j = 0; j < nbMachines; j++)
            {
                machines[j] = new IIntervalVar[nbJobs];
                sizes[j] = new int[nbJobs];
            }

            for (int i = 0; i < nbJobs; i++)
            {
                IIntervalVar prec = cp.IntervalVar();
                for (int j = 0; j < nbMachines; j++)
                {
                    int m, d;
                    m = data.Next();
                    d = data.Next();
                    IIntervalVar ti = cp.IntervalVar(0, d);
                    machines[m][i] = ti;
                    sizes[m][i] = d;
                    if (j > 0)
                    {
                        cp.Add(cp.EndBeforeStart(prec, ti));
                    }
                    prec = ti;
                }
                ends[i] = cp.EndOf(prec);
            }

            for (int j = 0; j < nbMachines; j++)
            {
                double alpha = data.Next() / ((double)100);
                IIntervalVar[] chain = new IIntervalVar[nbJobs];
                IIntervalVar prec = cp.IntervalVar();
                IIntExpr[] indices = new IIntExpr[nbJobs];
                for (int i = 0; i < nbJobs; i++)
                {
                    IIntervalVar syncti = cp.IntervalVar();
                    if (i > 0)
                    {
                        cp.Add(cp.EndBeforeStart(prec, syncti));
                    }
                    prec = syncti;
                    chain[i] = syncti;
                    IIntExpr index = cp.IntVar(0, nbJobs - 1);
                    indices[i] = index;
                    // Learning effect captured by the decreasing function
                    // of the position (0 <= alpha <= 1).
                    // At first position, in the sequence index = 0; there is no
                    // learning effect and duration of the task is its nominal duration
                    INumExpr floatDur = cp.Prod(sizes[j][i], cp.Power(alpha, index));
                    cp.Add(cp.Le(
                      cp.Abs(cp.Diff(floatDur, cp.SizeOf(machines[j][i]))),
                      0.5)
                    );
                }
                cp.Add(cp.Isomorphism(chain, machines[j], indices, nbJobs));
                // The no-overlap is a redundant constraint in this quite
                // simple model - it is used only to provide stronger inference.
                cp.Add(cp.NoOverlap(machines[j]));
            }

            IObjective objective = cp.Minimize(cp.Max(ends));
            cp.Add(objective);
            cp.SetParameter(CP.IntParam.LogPeriod, 10000);

            logger.Info("Instance \t: " + filename);
            if (cp.Solve())
            {
                logger.Info("Makespan \t: " + cp.ObjValue);
            }
            else
            {
                logger.Info("No solution found.");
            }
        }
    }
}
