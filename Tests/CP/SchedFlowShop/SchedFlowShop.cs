// -----------------------------------------------------------------*- C# -*-
// File: ./examples/src/csharp/SchedFlowShop.cs
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

This problem is a special case of Job-Shop Scheduling problem (see
SchedJobShop.cs) for which all jobs have the same processing order
on machines because there is a technological order on the machines for
the different jobs to follow.

------------------------------------------------------------ */

using System;
using System.IO;
using System.Collections.Generic;
using ILOG.CP;
using ILOG.Concert;
using COM.IBM.ML.ILOG;

namespace SchedFlowShop
{
    public class SchedFlowShop
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
            String filename = "../../../../../../Resources/cpo/flowshop_default.data";
            int failLimit = 10000;
            int nbJobs, nbMachines;

            if (args.Length > 0)
                filename = args[0];
            if (args.Length > 1)
                failLimit = Convert.ToInt32(args[1]);

            log4net.Config.BasicConfigurator.Configure();
            log4net.ILog logger =
                log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
       
            CP cp = new WmlCP(Credentials.GetCredentials(), Runtime.DO_20_1, TShirtSize.M, 1);
            DataReader data = new DataReader(filename);

            nbJobs = data.Next();
            nbMachines = data.Next();

            List<IIntExpr> ends = new List<IIntExpr>();
            List<IIntervalVar>[] machines = new List<IIntervalVar>[nbMachines];
            for (int j = 0; j < nbMachines; j++)
                machines[j] = new List<IIntervalVar>();

            for (int i = 0; i < nbJobs; i++)
            {
                IIntervalVar prec = cp.IntervalVar();
                for (int j = 0; j < nbMachines; j++)
                {
                    int d = data.Next();
                    IIntervalVar ti = cp.IntervalVar(d);
                    machines[j].Add(ti);
                    if (j > 0)
                    {
                        cp.Add(cp.EndBeforeStart(prec, ti));
                    }
                    prec = ti;
                }
                ends.Add(cp.EndOf(prec));
            }
            for (int j = 0; j < nbMachines; j++)
                cp.Add(cp.NoOverlap(machines[j].ToArray()));

            IObjective objective = cp.Minimize(cp.Max(ends.ToArray()));
            cp.Add(objective);

            cp.SetParameter(CP.IntParam.FailLimit, failLimit);
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
