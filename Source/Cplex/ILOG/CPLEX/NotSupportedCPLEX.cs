using ILOG.Concert;
using log4net;
using System;

namespace ILOG.CPLEX
{
    public abstract class NotSupportedCplex : Cplex
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(NotSupportedCplex));

        protected String notSupportedError = "Not supported by the CPLEX WML Connector";

        public NotSupportedCplex() : base()
        {
        }


        public override double GetObjValue(int var1)
        {
            logger.Error(notSupportedError + " GetObjValue");
            throw new ILOG.Concert.Exception(notSupportedError);
        }

        public override double GetBestObjValue()
        {
            logger.Error(notSupportedError + " GetBestObjValue");
            throw new ILOG.Concert.Exception(notSupportedError);
        }

        public override double GetMIPRelativeGap()
        {
            logger.Error(notSupportedError + " GetMIPRelativeGap");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override double[] GetValues(ILPMatrix matrix,
                                  int soln)
        {
            logger.Error(notSupportedError + " GetValues");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override double[] GetValues(INumVar[] var1, int var2, int var3, int var4)
        {
            logger.Error(notSupportedError + " GetValues");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override double GetValue(INumExpr var1, int var2)
        {
            logger.Error(notSupportedError + " GetValue");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override double[] GetValues(ILPMatrix matrix,
                                  int start,
                                  int num,
                                  int soln)
        {
            logger.Error(notSupportedError + " GetValues");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override double GetSolnPoolMeanObjValue()
        {
            logger.Error(notSupportedError + " GetSolnPoolMeanObjValue");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override int GetSolnPoolNsolns()
        {
            logger.Error(notSupportedError + " GetSolnPoolNsolns");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override int GetSolnPoolNreplaced()
        {
            logger.Error(notSupportedError + " GetSolnPoolNreplaced");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void DelSolnPoolSoln(int var1)
        {
            logger.Error(notSupportedError + " delSolnPoolSoln");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void DelSolnPoolSolns(int var1, int var2)
        {
            logger.Error(notSupportedError + " delSolnPoolSolns");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override double GetCutoff()
        {
            logger.Error(notSupportedError + " GetCutoff");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override Aborter Use(Aborter var1)
        {
            logger.Error(notSupportedError + " use aborter");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override Aborter GetAborter()
        {
            logger.Error(notSupportedError + " GetAborter");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void Remove(Aborter var1)
        {
            logger.Error(notSupportedError + " remove aborter");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override int TuneParam(string[] filenames, ParameterSet fixedset)
        {
            logger.Error(notSupportedError + " TuneParam");
            throw new ILOG.Concert.Exception(notSupportedError);
        }
        public override int TuneParam(ParameterSet fixedset)
        {
            logger.Error(notSupportedError + " TuneParam");
            throw new ILOG.Concert.Exception(notSupportedError);
        }
        public override int TuneParam()
        {
            logger.Error(notSupportedError + " TuneParam");
            throw new ILOG.Concert.Exception(notSupportedError);
        }
        public override int TuneParam(string[] filenames)
        {
            logger.Error(notSupportedError + " TuneParam");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void CopyVMConfig(String var1)
        {
            logger.Error(notSupportedError + " copyVMConfig");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void ReadVMConfig(String var1)
        {
            logger.Error(notSupportedError + " readVMConfig");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override bool HasVMConfig()
        {
            logger.Error(notSupportedError + " hasVMConfig");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void DelVMConfig()
        {
            logger.Error(notSupportedError + " delVMConfig");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override Goal EqGoal(INumExpr var1, double var2)
        {
            logger.Error(notSupportedError + " eqGoal");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override Goal EqGoal(INumExpr var1, INumExpr var2)
        {
            logger.Error(notSupportedError + " eqGoal");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override Goal EqGoal(double var1, INumExpr var3)
        {
            logger.Error(notSupportedError + " eqGoal");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override Goal GeGoal(INumExpr var1, double var2)
        {
            logger.Error(notSupportedError + " geGoal");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override Goal GeGoal(INumExpr var1, INumExpr var2)
        {
            logger.Error(notSupportedError + " geGoal");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override Goal GeGoal(double var1, INumExpr var3)
        {
            logger.Error(notSupportedError + " geGoal");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override Goal LeGoal(INumExpr var1, double var2)
        {
            logger.Error(notSupportedError + " leGoal");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override Goal LeGoal(INumExpr var1, INumExpr var2)
        {
            logger.Error(notSupportedError + " leGoal");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override Goal LeGoal(double var1, INumExpr var3)
        {
            logger.Error(notSupportedError + " leGoal");
            throw new ILOG.Concert.Exception(notSupportedError);
        }

 
        public override double GetInfeasibility(INumVar var1)
        {
            logger.Error(notSupportedError + " GetInfeasibility");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override BasisStatus GetBasisStatus(INumVar var1)
        {
            logger.Error(notSupportedError + " GetBasisStatus");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override BasisStatus[] GetBasisStatuses(INumVar[] var1)
        {
            logger.Error(notSupportedError + " GetBasisStatuses");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override BasisStatus[] GetBasisStatuses(INumVar[] var1, int var2, int var3)
        {
            logger.Error(notSupportedError + " GetBasisStatuses");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override BasisStatus GetBasisStatus(IConstraint var1)
        {
            logger.Error(notSupportedError + " GetBasisStatus");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override BasisStatus[] GetBasisStatuses(IConstraint[] var1)
        {
            logger.Error(notSupportedError + " GetBasisStatuses");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override BasisStatus[] GetBasisStatuses(IConstraint[] var1, int var2, int var3)
        {
            logger.Error(notSupportedError + " GetBasisStatuses");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override int GetNMIPStarts()
        {
            logger.Error(notSupportedError + " GetNMIPStarts");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void GetBoundSA(double[] var1, double[] var2, double[] var3, double[] var4, INumVar[] var5, int var6, int var7)
        {
            logger.Error(notSupportedError + " GetBoundSA");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void GetObjSA(double[] var1, double[] var2, INumVar[] var3, int var4, int var5)
        {
            logger.Error(notSupportedError + " GetObjSA");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void GetRangeSA(double[] var1, double[] var2, double[] var3, double[] var4, IRange[] var5, int var6, int var7)
        {
            logger.Error(notSupportedError + " GetRangeSA");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void GetRHSSA(double[] var1, double[] var2, IRange[] var3, int var4, int var5)
        {
            logger.Error(notSupportedError + " GetRHSSA");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void GetRHSSA(double[] var1, double[] var2, ILPMatrix var3, int var4, int var5)
        {
            logger.Error(notSupportedError + " GetRHSSA");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override int GetNcuts(int var1)
        {
            logger.Error(notSupportedError + " GetNcuts");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override int GetPriority(INumVar var1)
        {
            logger.Error(notSupportedError + " GetPriority");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override int[] GetPriorities(INumVar[] var1, int var2, int var3)
        {
            logger.Error(notSupportedError + " GetPriorities");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override double DualFarkas(IConstraint[] var1, double[] var2)
        {
            logger.Error(notSupportedError + " dualFarkas");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override bool RefineMIPStartConflict(int var1, IConstraint[] var2, double[] var3)
        {
            logger.Error(notSupportedError + " RefineMIPStartConflict");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override bool RefineMIPStartConflict(int var1, IConstraint[] var2, double[] var3, int var4, int var5)
        {
            logger.Error(notSupportedError + " RefineMIPStartConflict");
            throw new ILOG.Concert.Exception(notSupportedError);
        }



        public override bool Solve(ParameterSet[] var1)
        {
            logger.Error(notSupportedError + " solve with parameterset list");
            throw new ILOG.Concert.Exception(notSupportedError);
        }

        public override ILinearNumExpr GetQCDSlack(IRange var1)
        {
            logger.Error(notSupportedError + " GetQCDSlack");
            throw new ILOG.Concert.Exception(notSupportedError);
        }

        public override void Use(ModelingAssistance.Callback callback)
        {
            logger.Error(notSupportedError + " use Callback");
            throw new ILOG.Concert.Exception(notSupportedError);
        }
        public override void Use(Callback.Function callback, long contextMask)
        {
            logger.Error(notSupportedError + " use Callback");
            throw new ILOG.Concert.Exception(notSupportedError);
        }
        public override void Use(Callback var1)
        {
            logger.Error(notSupportedError + " use Callback");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void Remove(Callback var1)
        {
            logger.Error(notSupportedError + " remove Callback");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void ClearCallbacks()
        {
            logger.Error(notSupportedError + " clearCallbacks");
            throw new ILOG.Concert.Exception(notSupportedError);
        }



        public override void QpIndefCertificate(INumVar[] var1, double[] var2)
        {
            logger.Error(notSupportedError + " qpIndefCertificate");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void ProtectVariables(INumVar[] var1, int var2, int var3)
        {
            logger.Error(notSupportedError + " protectVariables");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override void ProtectVariables(INumVar[] var1)
        {
            logger.Error(notSupportedError + " protectVariables");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override double[] GetAX(ILPMatrix var1)
        {
            logger.Error(notSupportedError + " GetAX");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override double[] GetAX(ILPMatrix var1, int var2, int var3)
        {
            logger.Error(notSupportedError + " GetAX");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override double GetAX(IRange var1)
        {
            logger.Error(notSupportedError + " GetAX");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override double[] GetAX(IRange[] var1)
        {
            logger.Error(notSupportedError + " GetAX");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override double[] GetAX(IRange[] var1, int var2, int var3)
        {
            logger.Error(notSupportedError + " GetAX");
            throw new ILOG.Concert.Exception(notSupportedError);
        }

        public override Quality GetQuality(QualityType var1)
        {
            logger.Error(notSupportedError + " GetQuality");
            throw new ILOG.Concert.Exception(notSupportedError);
        }

        public override Quality GetQuality(QualityType var1, int var2)
        {
            logger.Error(notSupportedError + " GetQuality");
            throw new ILOG.Concert.Exception(notSupportedError);
        }
        public override BranchDirection GetDirection(INumVar var1)
        {
            logger.Error(notSupportedError + " GetDirection");
            throw new ILOG.Concert.Exception(notSupportedError);
        }

        public override BranchDirection[] GetDirections(INumVar[] var1)
        {
            logger.Error(notSupportedError + " GetDirections");
            throw new ILOG.Concert.Exception(notSupportedError);
        }

        public override BranchDirection[] GetDirections(INumVar[] var1, int var2, int var3)
        {
            logger.Error(notSupportedError + " GetDirections");
            throw new ILOG.Concert.Exception(notSupportedError);
        }

        public override ILinearNumExpr GetRay()
        {
            logger.Error(notSupportedError + " GetRay");
            throw new ILOG.Concert.Exception(notSupportedError);
        }

        public override ICopyable GetDiverging()
        {
            logger.Error(notSupportedError + " GetDiverging");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override int GetNLCs()
        {
            logger.Error(notSupportedError + " GetNLCs");
            throw new ILOG.Concert.Exception(notSupportedError);
        }


        public override int GetNUCs()
        {
            logger.Error(notSupportedError + " GetNUCs");
            throw new ILOG.Concert.Exception(notSupportedError);
        }

    }
}