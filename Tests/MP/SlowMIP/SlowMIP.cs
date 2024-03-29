

using COM.IBM.ML.ILOG;
using ILOG.Concert;
using ILOG.CPLEX;
using System.Collections;


public class SlowMIP
{
    public static void Main()
    {
        log4net.Config.BasicConfigurator.Configure();
        log4net.ILog logger =
            log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        logger.Info("Starting the example");

        logger.Info("Credentials are " + Credentials.GetCredentials());

        try
        {
            Cplex cplex = new WmlCplex(Credentials.GetCredentials(), Runtime.DO_20_1, TShirtSize.M, 1);

            int nx = 50;
            IIntVar[] boolVarArray = cplex.BoolVarArray(nx);
            INumVar[] numVarArray = cplex.NumVarArray(6, 0, 1000000);
            cplex.Minimize(cplex.Sum(numVarArray));
            cplex.Add(cplex.Le(cplex.Sum(numVarArray), 7));
            cplex.Add(
                    cplex.Eq(
                            cplex.Sum(numVarArray[0],
                                    cplex.ScalProd(
                                            boolVarArray,
                                            new int[]{
                                                25, 45, 14, 76, 58, 10, 20, 51, 58
                                                , 1, 35, 40, 65, 59, 24, 44, 1, 93
                                                , 24, 78, 38, 64, 93, 14, 83, 6
                                                , 58, 14, 71, 17, 18, 8, 57, 48
                                                , 35, 23, 47, 46, 8, 82, 51, 49
                                                , 85, 66, 45, 99, 21, 75, 78, 43
                                            }
                                    )
                            )
                            , 1116)
            );


            cplex.Add(
                    cplex.Eq(
                            cplex.Sum(numVarArray[1],
                                    cplex.ScalProd(boolVarArray, new int[]{
                                        97, 64, 24, 63, 58, 45, 25, 71, 32
                                        , 7, 28, 77, 94, 96, 70, 22, 93
                                        , 32, 17, 56, 74, 62, 94, 9, 92
                                        , 90, 40, 45, 74, 62, 62, 34, 21
                                        , 2, 75, 42, 75, 29, 4, 64, 80
                                        , 17, 55, 73, 23, 13, 91, 70, 73
                                        , 28
                                    })),
                            1325));

            cplex.Add(
                    cplex.Eq(
                            cplex.Sum(numVarArray[2],
                                    cplex.ScalProd(boolVarArray, new int[]{
                                        95, 71, 19, 15, 65, 76, 4, 50, 50
                                        , 97, 83, 14, 27, 14, 34, 9, 99
                                        , 62, 52, 39, 56, 53, 91, 81, 46
                                        , 94, 76, 53, 58, 23, 15, 63, 2
                                        , 31, 55, 71, 97, 71, 52, 8, 57
                                        , 14, 76, 1, 46, 87, 22, 97, 99
                                        , 92
                                    })),
                            1353));
            cplex.Add(
                    cplex.Eq(
                            cplex.Sum(numVarArray[3],
                                    cplex.ScalProd(boolVarArray, new int[]{
                                        1, 27, 46, 48, 66, 58, 52, 6, 14
                                        , 26, 55, 61, 60, 3, 33, 99, 36
                                        , 55, 70, 73, 70, 38, 66, 39, 43
                                        , 63, 88, 47, 18, 73, 40, 91, 96
                                        , 49, 13, 27, 22, 71, 99, 66, 57
                                        , 1, 54, 35, 52, 66, 26, 1, 26, 12
                                    })), 1169));

            cplex.Add(
                    cplex.Eq(
                            cplex.Sum(numVarArray[4],
                                    cplex.ScalProd(boolVarArray, new int[]{
                                        3, 94, 51, 4, 25, 46, 30, 2, 89
                                        , 65, 28, 46, 36, 53, 30, 73, 37
                                        , 60, 21, 41, 2, 21, 93, 82, 16
                                        , 97, 75, 50, 13, 43, 45, 64, 78
                                        , 78, 6, 35, 72, 31, 28, 56, 60
                                        , 23, 70, 46, 88, 20, 69, 13, 40
                                        , 73}))
                            , 1160));


            cplex.Add(
                    cplex.Eq(
                            cplex.Sum(
                                    numVarArray[5],
                                    cplex.ScalProd(boolVarArray, new int[]{69, 72, 94, 56, 90, 20, 56, 50, 79
                                        , 59, 36, 24, 42, 9, 29, 68, 10
                                        , 1, 44, 74, 61, 37, 71, 63, 44
                                        , 77, 57, 46, 51, 43, 4, 85, 59
                                        , 7, 25, 46, 25, 70, 78, 88, 20
                                        , 40, 40, 16, 3, 3, 5, 77, 88
                                        , 16})
                            )
                            , 1163)
            );

            cplex.SetParam(Cplex.Param.TimeLimit, 10);
            if (!cplex.Solve())
            {
                logger.Info(cplex.GetStatus());
                cplex.SetParam(Cplex.Param.TimeLimit, 10 * 60);
                if (cplex.Solve())
                {
                    logger.Info(cplex.GetStatus());
                    double[] reducedCosts = cplex.GetReducedCosts(numVarArray);
                    foreach (double d in reducedCosts)
                        logger.Info(d + " ");
                    double[] bools = cplex.GetValues(boolVarArray);
                    double[] nums = cplex.GetValues(numVarArray);
                    foreach (double d in bools)
                        logger.Info(d + " ");
                    foreach (double d in nums)
                        logger.Info(d + " ");
                }
            }
            else
            {
                throw new Exception("Should have failed with a small time limit");
            }
       

        cplex.End();
        }
        catch (ILOG.Concert.Exception e)
        {
            System.Console.WriteLine("Concert exception '" + e + "' caught");
        }
    }
}