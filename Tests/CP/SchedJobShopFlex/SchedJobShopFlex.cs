// -----------------------------------------------------------------*- C# -*-
// File: ./examples/src/csharp/SchedJobShopFlex.cs
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

This problem is an extension of the classical Job-Shop Scheduling
problem (see SchedJobShop.cs) which allows an operation to be
processed by any machine from a given set. The operation processing
time depends on the allocated machine. The problem is to assign each
operation to a machine and to order the operations on the machines
such that the maximal completion time (makespan) of all operations is
minimized.

------------------------------------------------------------ */

using System;
using System.IO;
using System.Collections.Generic;
using ILOG.CP;
using ILOG.Concert;
using COM.IBM.ML.ILOG;

namespace SchedJobShopFlex
{
    public class SchedJobShopFlex
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
            String filename = "../../../../../../Resources/cpo/jobshopflex_default.data";
            int failLimit = 10000;

            if (args.Length > 0)
                filename = args[0];
            if (args.Length > 1)
                failLimit = Convert.ToInt32(args[1]);

            log4net.Config.BasicConfigurator.Configure();
            log4net.ILog logger =
                log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            CP cp = new WmlCP(Credentials.GetCredentials(), Runtime.DO_20_1, TShirtSize.M, 1);

            DataReader data = new DataReader(filename);
            int nbJobs = data.Next();
            int nbMachines = data.Next();

            List<IIntervalVar>[] machines = new List<IIntervalVar>[nbMachines];
            for (int j = 0; j < nbMachines; j++)
                machines[j] = new List<IIntervalVar>();
            List<IIntExpr> ends = new List<IIntExpr>();

            for (int i = 0; i < nbJobs; i++)
            {
                int nbOperations = data.Next();
                IIntervalVar prec = cp.IntervalVar();
                for (int j = 0; j < nbOperations; j++)
                {
                    int nbOpMachines = data.Next();
                    IIntervalVar master = cp.IntervalVar();
                    List<IIntervalVar> members = new List<IIntervalVar>();
                    for (int k = 0; k < nbOpMachines; k++)
                    {
                        int m = data.Next();
                        int d = data.Next();
                        IIntervalVar member = cp.IntervalVar(d);
                        member.SetOptional();
                        members.Add(member);
                        machines[m - 1].Add(member);
                    }
                    cp.Add(cp.Alternative(master, members.ToArray()));
                    if (j > 0)
                        cp.Add(cp.EndBeforeStart(prec, master));
                    prec = master;
                }
                ends.Add(cp.EndOf(prec));
            }

            for (int j = 0; j < nbMachines; j++)
            {
                cp.Add(cp.NoOverlap(machines[j].ToArray()));
            }

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

