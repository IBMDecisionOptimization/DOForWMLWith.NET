using System;
using System.IO;
using ILOG.CP;
using ILOG.Concert;
using COM.IBM.ML.ILOG;
using System.Collections.Generic;

namespace SchedOptional
{
    public class SchedOptional
    {

        const int nbWorkers = 3;
        const int nbTasks = 10;

        const int joe = 0;
        const int jack = 1;
        const int jim = 2;

        static String[] workerNames = {
        "Joe",
        "Jack",
        "Jim"
    };

        const int masonry = 0;
        const int carpentry = 1;
        const int plumbing = 2;
        const int ceiling = 3;
        const int roofing = 4;
        const int painting = 5;
        const int windows = 6;
        const int facade = 7;
        const int garden = 8;
        const int moving = 9;

        static String[] taskNames = {
        "masonry",
        "carpentry",
        "plumbing",
        "ceiling",
        "roofing",
        "painting",
        "windows",
        "facade",
        "garden",
        "moving"
    };

        static int[] taskDurations = {
        35,
        15,
        40,
        15,
        05,
        10,
        05,
        10,
        05,
        05
    };

        static int[] skillsMatrix = {
        // Joe, Jack, Jim
        9, 5, 0, // masonry
        7, 0, 5, // carpentry
        0, 7, 0, // plumbing
        5, 8, 0, // ceiling
        6, 7, 0, // roofing
        0, 9, 6, // painting
        8, 0, 5, // windows
        5, 5, 0, // facade
        5, 5, 9, // garden
        6, 0, 8  // moving
    };

        public static bool HasSkill(int w, int i)
        {
            return (0 < skillsMatrix[nbWorkers * i + w]);
        }

        public static int SkillLevel(int w, int i)
        {
            return skillsMatrix[nbWorkers * i + w];
        }


        static INumExpr skill;

        public static void MakeHouse(CP cp, System.Collections.Generic.List<IIntervalVar> allTasks,
                                     List<IIntervalVar>[] workerTasks,
                                     int id,
                                     int deadline)
        {

            /* CREATE THE INTERVAL VARIABLES. */
            String name;
            IIntervalVar[] tasks = new IIntervalVar[nbTasks];
            IIntervalVar[,] taskMatrix = new IIntervalVar[nbTasks, nbWorkers];

            for (int i = 0; i < nbTasks; i++)
            {
                name = "H" + id + "-" + taskNames[i];
                tasks[i] = cp.IntervalVar(taskDurations[i], name);

                /* ALLOCATING TASKS TO WORKERS. */
                List<IIntervalVar> alttasks = new List<IIntervalVar>();
                for (int w = 0; w < nbWorkers; w++)
                {
                    if (HasSkill(w, i))
                    {
                        name = "H" + id + "-" + taskNames[i] + "-" + workerNames[w];
                        IIntervalVar wtask = cp.IntervalVar(taskDurations[i], name);
                        wtask.SetOptional();
                        alttasks.Add(wtask);
                        taskMatrix[i, w] = wtask;
                        workerTasks[w].Add(wtask);
                        allTasks.Add(wtask);
                        /* DEFINING MAXIMIZATION OBJECTIVE. */
                        skill = cp.Sum(skill, cp.Prod(SkillLevel(w, i), cp.PresenceOf(wtask)));
                    }
                }
                cp.Add(cp.Alternative(tasks[i], alttasks.ToArray()));
            }

            /* ADDING PRECEDENCE CONSTRAINTS. */
            tasks[moving].EndMax = deadline;
            cp.Add(cp.EndBeforeStart(tasks[masonry], tasks[carpentry]));
            cp.Add(cp.EndBeforeStart(tasks[masonry], tasks[plumbing]));
            cp.Add(cp.EndBeforeStart(tasks[masonry], tasks[ceiling]));
            cp.Add(cp.EndBeforeStart(tasks[carpentry], tasks[roofing]));
            cp.Add(cp.EndBeforeStart(tasks[ceiling], tasks[painting]));
            cp.Add(cp.EndBeforeStart(tasks[roofing], tasks[windows]));
            cp.Add(cp.EndBeforeStart(tasks[roofing], tasks[facade]));
            cp.Add(cp.EndBeforeStart(tasks[plumbing], tasks[facade]));
            cp.Add(cp.EndBeforeStart(tasks[roofing], tasks[garden]));
            cp.Add(cp.EndBeforeStart(tasks[plumbing], tasks[garden]));
            cp.Add(cp.EndBeforeStart(tasks[windows], tasks[moving]));
            cp.Add(cp.EndBeforeStart(tasks[facade], tasks[moving]));
            cp.Add(cp.EndBeforeStart(tasks[garden], tasks[moving]));
            cp.Add(cp.EndBeforeStart(tasks[painting], tasks[moving]));

            /* ADDING SAME-WORKER CONSTRAINTS. */
            cp.Add(cp.Add(cp.Equiv(cp.PresenceOf(taskMatrix[masonry, joe]),
                       cp.PresenceOf(taskMatrix[carpentry, joe]))));
            cp.Add(cp.Add(cp.Equiv(cp.PresenceOf(taskMatrix[roofing, jack]),
                       cp.PresenceOf(taskMatrix[facade, jack]))));
            cp.Add(cp.Add(cp.Equiv(cp.PresenceOf(taskMatrix[carpentry, joe]),
                       cp.PresenceOf(taskMatrix[roofing, joe]))));
            cp.Add(cp.Add(cp.Equiv(cp.PresenceOf(taskMatrix[garden, jim]),
                       cp.PresenceOf(taskMatrix[moving, jim]))));
        }

        public static void Main(String[] args)
        {
            log4net.Config.BasicConfigurator.Configure();
            log4net.ILog logger =
                log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            int nbHouses = 5;
            int deadline = 318;
            
            Credentials credentials = new Credentials(false);

            var IAM_HOST = "https://iam.cloud.ibm.com";
            var IAM_URL = "/identity/token";
            var WML_HOST = "https://us-south.ml.cloud.ibm.com";
            var WML_API_KEY = System.Environment.GetEnvironmentVariable("WML_API_KEY");
            var WML_SPACE_ID = System.Environment.GetEnvironmentVariable("WML_SPACE_ID");
            var WML_VERSION = "2021-06-01";
            var PLATFORM_HOST = "api.dataplatform.cloud.ibm.com";

            credentials.Add(Credentials.IAM_HOST, IAM_HOST);
            credentials.Add(Credentials.IAM_URL, IAM_URL);
            credentials.Add(Credentials.WML_HOST, WML_HOST);
            credentials.Add(Credentials.WML_API_KEY, WML_API_KEY);
            credentials.Add(Credentials.WML_SPACE_ID, WML_SPACE_ID);
            credentials.Add(Credentials.WML_VERSION, WML_VERSION);
            credentials.Add(Credentials.PLATFORM_HOST, PLATFORM_HOST);

            Runtime runtime = Runtime.DO_20_1;
            TShirtSize size = TShirtSize.M;
            int numNodes = 1;

            CP cp = new WmlCP(credentials, runtime, size, numNodes);


            skill = cp.IntExpr();
            List<IIntervalVar> allTasks = new List<IIntervalVar>();
            List<IIntervalVar>[] workerTasks = new List<IIntervalVar>[nbWorkers];
            for (int w = 0; w < nbWorkers; w++)
            {
                workerTasks[w] = new List<IIntervalVar>();
            }

            for (int h = 0; h < nbHouses; h++)
            {
                MakeHouse(cp, allTasks, workerTasks, h, deadline);
            }

            for (int w = 0; w < nbWorkers; w++)
            {
                IIntervalSequenceVar seq = cp.IntervalSequenceVar(workerTasks[w].ToArray(), workerNames[w]);
                cp.Add(cp.NoOverlap(seq));
            }

            cp.Add(cp.Maximize(skill));

            /* EXTRACTING THE MODEL AND SOLVING. */
            cp.SetParameter(CP.IntParam.FailLimit, 10000);
            if (cp.Solve())
            {
                logger.Info("Solution with objective " + cp.ObjValue + ":");
                for (int i = 0; i < allTasks.Count; i++)
                {
                    IIntervalVar var = (IIntervalVar)allTasks[i];
                    String name = ((IIntervalVar)allTasks[i]).Name;
                    if (cp.IsPresent(name))
                    {
                        logger.Info(
                            name + " is present with " +
                            "start: " + cp.GetStart(name) +
                            "end: " + cp.GetEnd(name) +
                            "length: " + cp.GetLength(name)
                            );
                    }
                    else
                        logger.Info(name + " is absent");
                        //logger.Info(cp.GetDomain((IIntervalVar)allTasks[i]));
                }
            }
            else
            {
                logger.Info("No solution found. ");
            }
        }
    }
}
