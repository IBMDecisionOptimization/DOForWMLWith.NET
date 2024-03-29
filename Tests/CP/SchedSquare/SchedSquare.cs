// -----------------------------------------------------------------*- C# -*-
// File: ./examples/src/csharp/SchedSquare.cs
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

The aim of the square example is to place a set of small squares of
different sizes into a large square.

------------------------------------------------------------ */

using System;
using ILOG.CP;
using ILOG.Concert;
using COM.IBM.ML.ILOG;

namespace SchedSquare
{
    public class SchedSquare
    {
        public static void Main(String[] args)
        {
            log4net.Config.BasicConfigurator.Configure();
            log4net.ILog logger =
                log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            try
            {
                CP cp = new WmlCP(Credentials.GetCredentials(), Runtime.DO_20_1, TShirtSize.M, 1);

                int sizeSquare = 112;
                int nbSquares = 21;
                int[] size = { 50, 42, 37, 35, 33, 29, 27, 25, 24, 19, 18, 17, 16, 15, 11, 9, 8, 7, 6, 4, 2 };
                IIntervalVar[] x = new IIntervalVar[nbSquares];
                IIntervalVar[] y = new IIntervalVar[nbSquares];
                ICumulFunctionExpr rx = cp.CumulFunctionExpr();
                ICumulFunctionExpr ry = cp.CumulFunctionExpr();

                for (int i = 0; i < nbSquares; ++i)
                {
                    x[i] = cp.IntervalVar(size[i], "X" + i);
                    x[i].EndMax = sizeSquare;
                    y[i] = cp.IntervalVar(size[i], "Y" + i);
                    y[i].EndMax = sizeSquare;
                    rx = cp.Sum(rx, cp.Pulse(x[i], size[i]));
                    ry = cp.Sum(ry, cp.Pulse(y[i], size[i]));

                    for (int j = 0; j < i; ++j)
                    {
                        cp.Add(cp.Or(cp.Le(cp.EndOf(x[i]), cp.StartOf(x[j])),
                                cp.Or(cp.Le(cp.EndOf(x[j]), cp.StartOf(x[i])),
                                        cp.Or(cp.Le(cp.EndOf(y[i]), cp.StartOf(y[j])),
                                                cp.Le(cp.EndOf(y[j]), cp.StartOf(y[i]))))));
                    }
                }
                cp.Add(cp.AlwaysIn(rx, 0, sizeSquare, sizeSquare, sizeSquare));
                cp.Add(cp.AlwaysIn(ry, 0, sizeSquare, sizeSquare, sizeSquare));

                ISearchPhase[] phases = new ISearchPhase[2];
                phases[0] = cp.SearchPhase(x);
                phases[1] = cp.SearchPhase(y);

                cp.SetSearchPhases(phases);
                if (cp.Solve())
                {
                    for (int i = 0; i < nbSquares; ++i)
                    {
                        logger.Info("Square " + i + ": ["
                                + cp.GetStart(x[i].Name) + "," + cp.GetEnd(x[i].Name)
                                + "] x ["
                                + cp.GetStart(y[i].Name) + "," + cp.GetEnd(y[i].Name)
                                + "]");
                    }
                }
            }
            catch (ILOG.Concert.Exception ex)
            {
                logger.Info("Caught: " + ex);
            }
        }
    }
}

