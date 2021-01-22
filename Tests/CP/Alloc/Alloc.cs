// -----------------------------------------------------------------*- C# -*-
// File: ./examples/src/csharp/Alloc.cs
// --------------------------------------------------------------------------
// Licensed Materials - Property of IBM
//
// 5724-Y48 5724-Y49 5724-Y54 5724-Y55 5725-A06 5725-A29
// Copyright IBM Corporation 1990, 2020. All Rights Reserved.
//
// Note to U.S. Government Users Restricted Rights:
// Use, duplication or disclosure restricted by GSA ADP Schedule
// Contract with IBM Corp.
// --------------------------------------------------------------------------

/* ------------------------------------------------------------

Frequency assignment problem
----------------------------

Problem Description

The problem is given here in the form of discrete data; that is,
each frequency is represented by a number that can be called its
channel number.  For practical purposes, the network is divided
into cells (this problem is an actual cellular phone problem).
In each cell, there is a transmitter which uses different
channels.  The shape of the cells have been determined, as well
as the precise location where the transmitters will be
installed.  For each of these cells, traffic requires a number
of frequencies.

Between two cells, the distance between frequencies is given in
the matrix on the next page.

The problem of frequency assignment is to avoid interference.
As a consequence, the distance between the frequencies within a
cell must be greater than 16.  To avoid inter-cell interference,
the distance must vary because of the geography.

------------------------------------------------------------ */

using System;
using System.IO;
using ILOG.CP;
using ILOG.Concert;
using COM.IBM.ML.ILOG;

namespace Alloc
{
    public class Alloc
    {
        static int nbCell = 25;
        static int nbAvailFreq = 256;
        static int[] nbChannel = {
        8,6,6,1,4,4,8,8,8,8,4,9,8,4,4,10,8,9,8,4,5,4,8,1,1
      };
        static int[][] dist = new int[25][];

        public static int getTransmitterIndex(int cell, int channel)
        {
            int idx = 0;
            int c = 0;
            while (c < cell)
                idx += nbChannel[c++];
            return (idx + channel);
        }

        public static void Main(String[] args)
        {
            log4net.Config.BasicConfigurator.Configure();
            log4net.ILog logger =
                log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            int numPeriods = 6;
            if (args.Length > 0)
                numPeriods = Int32.Parse(args[0]);


            CP cp = new WmlCP(Credentials.GetCredentials(), Runtime.DO_12_10, TShirtSize.M, 1);
            dist[0] = new int[] { 16, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 1, 1, 0, 0, 0, 2, 2, 1, 1, 1 };
            dist[1] = new int[] { 1, 16, 2, 0, 0, 0, 0, 0, 2, 2, 1, 1, 1, 2, 2, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
            dist[2] = new int[] { 1, 2, 16, 0, 0, 0, 0, 0, 2, 2, 1, 1, 1, 2, 2, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
            dist[3] = new int[] { 0, 0, 0, 16, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1 };
            dist[4] = new int[] { 0, 0, 0, 2, 16, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1 };
            dist[5] = new int[] { 0, 0, 0, 2, 2, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1 };
            dist[6] = new int[] { 0, 0, 0, 0, 0, 0, 16, 2, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 2, 0, 0, 0, 1, 1 };
            dist[7] = new int[] { 0, 0, 0, 0, 0, 0, 2, 16, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 2, 0, 0, 0, 1, 1 };
            dist[8] = new int[] { 1, 2, 2, 0, 0, 0, 0, 0, 16, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1 };
            dist[9] = new int[] { 1, 2, 2, 0, 0, 0, 0, 0, 2, 16, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1 };
            dist[10] = new int[] { 1, 1, 1, 0, 0, 0, 1, 1, 2, 2, 16, 2, 2, 2, 2, 2, 2, 1, 1, 2, 1, 1, 0, 1, 1 };
            dist[11] = new int[] { 1, 1, 1, 0, 0, 0, 1, 1, 2, 2, 2, 16, 2, 2, 2, 2, 2, 1, 1, 2, 1, 1, 0, 1, 1 };
            dist[12] = new int[] { 1, 1, 1, 0, 0, 0, 1, 1, 2, 2, 2, 2, 16, 2, 2, 2, 2, 1, 1, 2, 1, 1, 0, 1, 1 };
            dist[13] = new int[] { 2, 2, 2, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 16, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            dist[14] = new int[] { 2, 2, 2, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 16, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            dist[15] = new int[] { 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 1, 1, 16, 2, 2, 2, 1, 2, 2, 1, 2, 2 };
            dist[16] = new int[] { 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 1, 1, 2, 16, 2, 2, 1, 2, 2, 1, 2, 2 };
            dist[17] = new int[] { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 16, 2, 2, 1, 1, 0, 2, 2 };
            dist[18] = new int[] { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 16, 2, 1, 1, 0, 2, 2 };
            dist[19] = new int[] { 0, 0, 0, 1, 1, 1, 2, 2, 1, 1, 2, 2, 2, 1, 1, 1, 1, 2, 2, 16, 1, 1, 0, 1, 1 };
            dist[20] = new int[] { 2, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 1, 16, 2, 1, 2, 2 };
            dist[21] = new int[] { 2, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 1, 2, 16, 1, 2, 2 };
            dist[22] = new int[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 16, 1, 1 };
            dist[23] = new int[] { 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 1, 2, 2, 1, 16, 2 };
            dist[24] = new int[] { 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 1, 2, 2, 1, 2, 16 };

            int nbTransmitters = getTransmitterIndex(nbCell, 0);
            IIntVar[] freq = cp.IntVarArray(nbTransmitters,
                                            0, nbAvailFreq - 1, "freq");
            for (int cell = 0; cell < nbCell; cell++)
                for (int channel1 = 0; channel1 < nbChannel[cell]; channel1++)
                    for (int channel2 = channel1 + 1; channel2 < nbChannel[cell]; channel2++)
                        cp.Add(cp.Ge(cp.Abs(cp.Diff(freq[getTransmitterIndex(cell, channel1)],
                                                    freq[getTransmitterIndex(cell, channel2)])),
                                     16));
            for (int cell1 = 0; cell1 < nbCell; cell1++)
                for (int cell2 = cell1 + 1; cell2 < nbCell; cell2++)
                    if (dist[cell1][cell2] > 0)
                        for (int channel1 = 0; channel1 < nbChannel[cell1]; channel1++)
                            for (int channel2 = 0; channel2 < nbChannel[cell2]; channel2++)
                                cp.Add(cp.Ge(cp.Abs(cp.Diff(freq[getTransmitterIndex(cell1, channel1)],
                                                            freq[getTransmitterIndex(cell2, channel2)])),
                                             dist[cell1][cell2]));

            // Minimizing the total number of frequencies
            IIntExpr nbFreq = cp.CountDifferent(freq);
            cp.Add(cp.Minimize(nbFreq));

            cp.SetParameter(CP.IntParam.CountDifferentInferenceLevel,
                            CP.ParameterValues.Extended);
            cp.SetParameter(CP.IntParam.FailLimit, 40000);
            cp.SetParameter(CP.IntParam.LogPeriod, 100000);
            IIntVar nbFreqVar = cp.IntVar(0,int.MaxValue);
            cp.Add(cp.Eq(nbFreq, nbFreqVar));

            if (cp.Solve())
            {
                for (int cell = 0; cell < nbCell; cell++)
                {
                    for (int channel = 0; channel < nbChannel[cell]; channel++)
                        logger.Info(cp.GetIntValue(freq[getTransmitterIndex(cell, channel)])
                                      + "  ");
                    logger.Info("");
                }
                logger.Info("Total # of sites       " + nbTransmitters);
                logger.Info("Total # of frequencies " + cp.GetValue(nbFreqVar));

            }
            else
                logger.Info("No solution");
            cp.End();
        }
    }
}

