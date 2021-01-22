using log4net;
using System;
using ILOG.Concert;
using System.IO;

namespace ILOG.CP
{

    public class NotSupportedCP : CP
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(NotSupportedCP));
        protected String notSupportedError = "Not supported by the CPO WML Connector";


        public NotSupportedCP() : base()
        {
        }

        /************************************************************************
         * Functions for managing: Explanations
         ************************************************************************/
        public override double ObjGap
        {
            get
            {
                logger.Error(notSupportedError + " ObjGap");
                throw new Concert.Exception(notSupportedError);
            }
        }

        public override void ClearExplanations()
        {
            logger.Error(notSupportedError + " clearExplanations");
            throw new Concert.Exception(notSupportedError);
        }


        public override void ExplainFailure(int failTag)
        {
            logger.Error(notSupportedError + " explainFailure");
            throw new Concert.Exception(notSupportedError);
        }


        public override void ExplainFailure(int[] failTags)
        {
            logger.Error(notSupportedError + " explainFailure");
            throw new Concert.Exception(notSupportedError);
        }


        /************************************************************************
         * Functions for retrieving search information
         ************************************************************************/

        public override double GetInfo(DoubleInfo which)
        {
            logger.Error(notSupportedError + " GetInfo");
            throw new Concert.Exception(notSupportedError);
        }


        public override int GetInfo(IntInfo which)
        {
            logger.Error(notSupportedError + " GetInfo");
            throw new Concert.Exception(notSupportedError);
        }


        /************************************************************************
         * Functions for Getting values on Expressions
         ************************************************************************/

        public override double GetValue(Concert.IIntExpr i)
        {
            logger.Error(notSupportedError + " GetValue");
            throw new Concert.Exception(notSupportedError);
        }


        public override double GetValue(Concert.INumExpr num)
        {
            logger.Error(notSupportedError + " GetValue");
            throw new Concert.Exception(notSupportedError);
        }

        /*
    public override double GetIncumbentValue(Concert.INumExpr expr) 
{
    logger.Error(notSupportedError + " GetIncumbentValue");
        throw new Concert.Exception(notSupportedError);
	}

	
    public override double GetIncumbentValue(String exprName) 
{
    logger.Error(notSupportedError + " GetIncumbentValue");
        throw new Concert.Exception(notSupportedError);
	}
        */

        /************************************************************************
         * Functions for Getting segment values for Cumulative Functions
         ************************************************************************/
        /*
        public override int GetNumberOfSegments(ICumulFunctionExpr f) 
    {
        logger.Error(notSupportedError + " GetNumberOfSegments");
            throw new Concert.Exception(notSupportedError);
        }


        public override int GetSegmentStart(ICumulFunctionExpr f, int i) 
    {
        logger.Error(notSupportedError + " GetSegmentStart");
            throw new Concert.Exception(notSupportedError);
        }

        public override int GetSegmentEnd(ICumulFunctionExpr f, int i) 
    {
        logger.Error(notSupportedError + " GetSegmentEnd");
            throw new Concert.Exception(notSupportedError);
        }


        public override int GetSegmentValue(ICumulFunctionExpr f, int i) 
    {
        logger.Error(notSupportedError + " GetSegmentValue");
            throw new Concert.Exception(notSupportedError);
        }


        public override int GetValue(ICumulFunctionExpr f, int t) 
    {
        logger.Error(notSupportedError + " GetValue");
            throw new Concert.Exception(notSupportedError);
        }


        public override bool IsFixed(ICumulFunctionExpr f) 
    {
        logger.Error(notSupportedError + " isFixed");
            throw new Concert.Exception(notSupportedError);
        }

        */

        /************************************************************************
         * Functions for managing: Conflicts
         ************************************************************************/

        public override bool RefineConflict(IConstraint[] csts)
        {
            logger.Error(notSupportedError + " refineConflict");
            throw new Concert.Exception(notSupportedError);
        }


        public override bool RefineConflict(IConstraint[] csts, double[] prefs)
        {
            logger.Error(notSupportedError + " refineConflict");
            throw new Concert.Exception(notSupportedError);
        }



        public override ConflictStatus GetConflict(IConstraint cst)
        {
            logger.Error(notSupportedError + " GetConflict");
            throw new Concert.Exception(notSupportedError);
        }


        public override ConflictStatus GetConflict(INumVar var)
        {
            logger.Error(notSupportedError + " GetConflict");
            throw new Concert.Exception(notSupportedError);
        }


        public override ConflictStatus GetConflict(IIntervalVar var)
        {
            logger.Error(notSupportedError + " GetConflict");
            throw new Concert.Exception(notSupportedError);
        }


        public override void ExportConflict(TextWriter s)
        {
            logger.Error(notSupportedError + " exportConflict");
            throw new Concert.Exception(notSupportedError);
        }


        /************************************************************************
         * Functions for controlling search and performing statistics
         ************************************************************************/

        public override void RunSeeds(int n)
        {
            logger.Error(notSupportedError + " runSeeds");
            throw new Concert.Exception(notSupportedError);
        }


        public override void RunSeeds()
        {
            logger.Error(notSupportedError + " runSeeds");
            throw new Concert.Exception(notSupportedError);
        }


        public override void StartNewSearch()
        {
            logger.Error(notSupportedError + " startNewSearch");
            throw new Concert.Exception(notSupportedError);
        }


        public override bool Next()
        {
            logger.Error(notSupportedError + " next");
            throw new Concert.Exception(notSupportedError);
        }

    }
}
